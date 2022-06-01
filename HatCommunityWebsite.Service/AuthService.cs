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
        AuthenticateResponse Authenticate(LogInDto request, string ipAddress);

        void Register(UserDto request, string origin);

        void VerifyEmail(string token);

        AuthenticateResponse RefreshToken(string token, string ipAddress);

        void ForgotPassword(ForgotPasswordDto request, string origin);

        void ResetPassword(ResetPasswordDto request);
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

        public AuthenticateResponse Authenticate(LogInDto request, string ipAddress)
        {
            var user = _userRepo.GetUserByUsername(request.Username).Result;

            // validate
            if (user == null || !user.IsVerified || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new AppException("Email or password is incorrect");

            // authentication successful so generate jwt and refresh token
            var jwtToken = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);

            // add refresh token to account
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpires = refreshToken.Expires;
            user.RefreshTokenCreated = refreshToken.Created;

            // update changes to db
            _userRepo.UpdateUser(user);

            var response = new AuthenticateResponse
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken
            };

            return response;
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var user = _userRepo.GetUserByRefreshToken(token).Result;

            if (user == null)
                throw new AppException("Invalid token");

            if (user.RefreshTokenExpires < DateTime.Now) //is this necessary?
                throw new AppException("Token expired");

            // replace old refresh token with a new one
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpires = newRefreshToken.Expires;
            user.RefreshTokenCreated = newRefreshToken.Created;

            // save changes to db
            _userRepo.UpdateUser(user);

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            // return data in authenticate response object
            var response = new AuthenticateResponse
            {
                JwtToken = jwtToken,
                RefreshToken = newRefreshToken
            };

            return response;
        }

        public void Register(UserDto request, string origin)
        {
            // validate email
            if (_userRepo.UserExistsByEmail(request.Email).Result)
                throw new AppException("Email already exists.");

            // validate username
            if (_userRepo.UserExistsByEmail(request.Username).Result)
                throw new AppException("Username already exists.");

            var user = new User();
            user.Username = request.Username;
            user.Email = request.Email;  
            user.Role = (int)UserRoles.ROLE_BASIC;
            user.Created = DateTime.UtcNow;
            user.VerificationToken = GenerateVerificationToken();
            user.IsImported = false;

            // hash password
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // save user
            _userRepo.SaveUser(user);

            // send verification email
            SendVerificationEmail(user, origin);
        }

        public void VerifyEmail(string token)
        {
            var user = _userRepo.GetUserByVerificationToken(token).Result;

            if (user == null)
                throw new AppException("Verification failed");

            user.VerifiedDate = DateTime.UtcNow;
            user.VerificationToken = null;

            _userRepo.UpdateUser(user);
        }

        public void ForgotPassword(ForgotPasswordDto request, string origin)
        {
            var user = _userRepo.GetUserByEmail(request.Email).Result;

            // always return ok response to prevent email enumeration
            if (user == null) return;

            // create reset token that expires after 1 day
            user.ResetPasswordToken = GenerateResetPasswordToken();
            user.ResetPasswordTokenExpires = DateTime.UtcNow.AddDays(1);

            _userRepo.UpdateUser(user);

            SendPasswordResetEmail(user, origin);
        }

        public void ResetPassword(ResetPasswordDto request)
        {
            var user = _userRepo.GetUserByResetPasswordToken(request.Token).Result;
            if (user == null) throw new AppException("Invalid reset password token");

            // update password and remove reset token
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpires = null;

            _userRepo.UpdateUser(user);
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

        private string GenerateVerificationToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            // ensure token is unique by checking against db
            var tokenIsUnique = !_userRepo.UserVerificationTokenIsUnique(token).Result;
            if (!tokenIsUnique)
                return GenerateVerificationToken();

            return token;
        }

        private string GenerateResetPasswordToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            // ensure token is unique by checking against db
            var tokenIsUnique = !_userRepo.UserResetPasswordTokenIsUnique(token).Result;
            if (!tokenIsUnique)
                return GenerateResetPasswordToken();

            return token;
        }

        private void SendVerificationEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                // origin exists if request sent from browser
                // sending link to verify through browser
                var verifyUrl = $"{origin}/user/verify-email?token={user.VerificationToken}";
                message = $@"<p>Click the below link to verify your account:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
            }
            else
            {
                // origin missing if request sent directly to api with some restful app
                // sending instructions to verify account through the api
                message = $@"<p>Please use the below token to verify your email address with the <code>/user/verify-email</code> api route:</p>
                            <p><code>{user.VerificationToken}</code></p>";
            }

            _emailService.Send(
                to: user.Email,
                subject: "Hatruns.com - Account Verification",
                html: $@"<h4>Verify Email</h4>
                        <p>Thanks for registering!</p>
                        {message}"
            );
        }

        private void SendPasswordResetEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/user/reset-password?token={user.ResetPasswordToken}";
                message = $@"<p>Please click the below link to reset your password. The link will expire after 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/user/reset-password</code> api route. The token will expire after 1 day:</p>
                            <p><code>{user.ResetPasswordToken}</code></p>";
            }

            _emailService.Send(
                to: user.Email,
                subject: "Hatruns.com - Reset Password",
                html: $@"<h4>Reset Password</h4>
                        {message}"
            );
        }
    }
}