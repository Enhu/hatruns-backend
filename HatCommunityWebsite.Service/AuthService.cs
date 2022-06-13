using AutoMapper;
using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Helpers;
using HatCommunityWebsite.Service.Responses;
using System.Security.Cryptography;
using System.Text;

namespace HatCommunityWebsite.Service
{
    public interface IAuthService
    {
        Task<AuthenticateResponse> Authenticate(LogInDto request, string ipAddress);

        Task Register(UserDto request, string origin);

        Task VerifyEmail(string token);

        Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);

        Task ForgotPassword(ForgotPasswordDto request, string origin);

        Task ResetPassword(ResetPasswordDto request);

        Task ValidateResetPassword(ValidateResetPasswordDto request);

        Task Logout(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IJwtUtils _jwtUtils;

        public AuthService(IUserRepository repo, IEmailService emailService, IMapper mapper, IJwtUtils jwtUtils)
        {
            _userRepo = repo;
            _emailService = emailService;
            _mapper = mapper;
            _jwtUtils = jwtUtils;
        }

        public async Task<AuthenticateResponse> Authenticate(LogInDto request, string ipAddress)
        {
            var user = await _userRepo.GetUserByUsername(request.Username);

            // validate
            if (user == null || !user.IsVerified || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new AppException("Email or password is incorrect");

            //remember me option
            user.RememberLogIn = request.Remember;

            // authentication successful so generate jwt and refresh token
            var jwtToken = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);

            // add refresh token to account
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpires = refreshToken.Expires;
            user.RefreshTokenCreated = refreshToken.Created;

            // update changes to db
            await _userRepo.UpdateUser(user);

            var response = new AuthenticateResponse
            {
                JwtToken = jwtToken,
                Username = user.Username,
                IsAdmin = user.Role == (int)UserRoles.ROLE_ADMIN ? true : false,
                RefreshToken = refreshToken
            };

            return response;
        }

        public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
        {
            if (token == null)
                throw new AppException("Token cannot be null");

            var user = await _userRepo.GetUserByRefreshToken(token);

            if (user == null)
                throw new AppException("Invalid token");

            if (!user.RememberLogIn && user.RefreshTokenExpires < DateTime.Now)
            {
                user.RefreshToken = null;
                user.RefreshTokenCreated = null;

                await _userRepo.UpdateUser(user);

                throw new AppException("Token expired");
            }

            // replace old refresh token with a new one
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpires = newRefreshToken.Expires;
            user.RefreshTokenCreated = newRefreshToken.Created;

            // save changes to db
            await _userRepo.UpdateUser(user);

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            // return data in authenticate response object
            var response = new AuthenticateResponse
            {
                JwtToken = jwtToken,
                Username = user.Username,
                IsAdmin = user.Role == (int)UserRoles.ROLE_ADMIN ? true : false,
                RefreshToken = newRefreshToken
            };

            return response;
        }

        public async Task Logout(string token)
        {
            if (token == null)
                throw new AppException("Token cannot be null");

            var user = await _userRepo.GetUserByRefreshToken(token);

            if (user == null)
                throw new AppException("Invalid token");

            user.RefreshToken = null;
            user.RefreshTokenCreated = null;

            await _userRepo.UpdateUser(user);
        }

        public async Task Register(UserDto request, string origin)
        {
            // validate email
            var emailExists = await _userRepo.UserExistsByEmail(request.Email);
            if (emailExists)
                throw new AppException("Email already exists.");

            // validate username
            var userExists = await _userRepo.UserExistsByEmail(request.Username);
            if (userExists)
                throw new AppException("Username already exists.");

            var user = new User();
            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = (int)UserRoles.ROLE_BASIC;
            user.Created = DateTime.UtcNow;
            user.VerificationToken = await GenerateVerificationToken();
            user.IsImported = false;

            // hash password
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // save user
            await _userRepo.SaveUser(user);

            // send verification email
            SendVerificationEmail(user, origin);
        }

        public async Task VerifyEmail(string token)
        {
            var user = await _userRepo.GetUserByVerificationToken(token);

            if (user == null)
                throw new AppException("Verification failed");

            user.VerifiedDate = DateTime.UtcNow;
            user.VerificationToken = null;

            await _userRepo.UpdateUser(user);
        }

        public async Task ForgotPassword(ForgotPasswordDto request, string origin)
        {
            var user = await _userRepo.GetUserByEmail(request.Email);

            // always return ok response to prevent email enumeration
            if (user == null) return;

            // create reset token that expires after 1 day
            user.ResetPasswordToken = await GenerateResetPasswordToken();
            user.ResetPasswordTokenExpires = DateTime.UtcNow.AddDays(1);

            await _userRepo.UpdateUser(user);

            SendPasswordResetEmail(user, origin);
        }

        public async Task ValidateResetPassword(ValidateResetPasswordDto request)
        {
            var user = await _userRepo.GetUserByResetPasswordToken(request.Token);
            if (user == null) throw new AppException("Invalid reset password token.");

            if (!user.ResetTokenExpired)
                throw new AppException("Reset token expired. Send a new recovery email.");
        }

        public async Task ResetPassword(ResetPasswordDto request)
        {
            var user = await _userRepo.GetUserByResetPasswordToken(request.Token);
            if (user == null) throw new AppException("Invalid reset password token");

            if (!user.ResetTokenExpired)
                throw new AppException("Reset token expired. Send a new recovery email.");

            // update password and remove reset token
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpires = null;

            await _userRepo.UpdateUser(user);
        }

        //helper methods

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private async Task<string> GenerateVerificationToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            // ensure token is unique by checking against db
            var tokenIsUnique = !await _userRepo.UserVerificationTokenIsUnique(token);
            if (!tokenIsUnique)
                return await GenerateVerificationToken();

            return token;
        }

        private async Task<string> GenerateResetPasswordToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            // ensure token is unique by checking against db
            var tokenIsUnique = !await _userRepo.UserResetPasswordTokenIsUnique(token);
            if (!tokenIsUnique)
                return await GenerateResetPasswordToken();

            return token;
        }

        private void SendVerificationEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                // origin exists if request sent from browser
                // sending link to verify through browser
                var verifyUrl = $"{origin}/verify-account?token={user.VerificationToken}";
                message = $@"<p>Click the below link to verify your account:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
            }
            else
            {
                // origin missing if request sent directly to api with some restful app
                // sending instructions to verify account through the api
                message = $@"<p>Please use the below token to verify your email address with the <code>/verify-account</code> api route:</p>
                            <p><code>{user.VerificationToken}</code></p>";
            }

            _emailService.Send(
                to: user.Email,
                subject: "Hatruns.com - Account Verification",
                html: $@"<h4>Verify account</h4>
                        <p>Hello {user.Username}, thanks for registering!</p>
                        {message}"
            );
        }

        private void SendPasswordResetEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/reset-password?token={user.ResetPasswordToken}";
                message = $@"<p>Please click the below link to reset your password. The link will expire after 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/reset-password</code> api route. The token will expire after 1 day:</p>
                            <p><code>{user.ResetPasswordToken}</code></p>";
            }

            _emailService.Send(
                to: user.Email,
                subject: "Hatruns.com - Reset Password",
                html: $@"<h4>Reset password</h4>
                        <p>Hello {user.Username}, thanks for registering!</p>
                        {message}"
            );
        }
    }
}