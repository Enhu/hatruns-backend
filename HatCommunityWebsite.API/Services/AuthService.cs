//using FullRuns.DB;
//using System.Security.Cryptography;
//using System.Security.Claims;
//using System.Text;
//using HatCommunityWebsite.DB;
//using AutoMapper;

//namespace HatCommunityWebsite.API.Services
//{
//    public class AuthService
//    {
//        private readonly AppDbContext _context;
//        private readonly IMapper _mapper;
//        private enum Roles
//        {
//            ROLE_BASIC = 0,
//            ROLE_VERIFIER = 1,
//            ROLE_ADMIN = 2
//        }
//        public AuthService(AppDbContext context)
//        {
//            _context = context;
//        }


//        public void Register(UserDto request, string origin)
//        {
//            // validate email
//            if (_context.Users.Any(x => x.Email == request.Email))
//            {
//                //sendAlreadyRegisteredEmail(request.Email, origin);
//                return;
//            }

//            // validate username
//            if (_context.Users.Any(x => x.Username == request.Username))
//            {
//                //sendAlreadyRegisteredUsername(request.Username, origin);
//                return;
//            }

//            var user = new User();
//            // first registered account is an admin
//            var isFirstAccount = _context.Users.Count() == 0;
//            user.Username = request.Username;
//            user.Email = request.Email;
//            user.Role = isFirstAccount ? (int)Roles.ROLE_ADMIN : (int)Roles.ROLE_BASIC;
//            user.Created = DateTime.UtcNow;
//            user.VerificationToken = generateVerificationToken();

//            // hash password
//            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

//            user.PasswordHash = passwordHash;
//            user.PasswordSalt = passwordSalt;

//            // save account
//            _context.Users.Add(user);
//            _context.SaveChanges();

//            // send verification email
//            sendVerificationEmail(user, origin);
//        }
//        public void ForgotPassword(ForgotPasswordRequest model, string origin)
//        {
//            var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

//            // always return ok response to prevent email enumeration
//            if (account == null) return;

//            // create reset token that expires after 1 day
//            account.ResetToken = generateResetToken();
//            account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

//            _context.Accounts.Update(account);
//            _context.SaveChanges();

//            // send email
//            sendPasswordResetEmail(account, origin);
//        }

//        public void ResetPassword(ResetPasswordRequest model)
//        {
//            var user = getAccountByResetToken(model.Token);

//            // update password and remove reset token
//            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

//            user.PasswordHash = passwordHash;
//            user.PasswordSalt = passwordSalt;

//            user.PasswordHash = BCrypt.HashPassword(model.Password);
//            user.ResetPasswordToken = null;
//            user.ResetPasswordTokenExpires = null;

//            _context.Users.Update(user);
//            _context.SaveChanges();
//        }

//        private User getAccountByResetToken(string token)
//        {
//            var user = _context.Users.SingleOrDefault(x =>
//                x.ResetPasswordToken == token && x.ResetPasswordTokenExpires > DateTime.UtcNow);
//            if (user == null) throw new Exception("Invalid token"); //add appexception here
//            return user;
//        }


//        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
//        {
//            using (var hmac = new HMACSHA512())
//            {
//                passwordSalt = hmac.Key;
//                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
//            }
//        }

//        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
//        {
//            using (var hmac = new HMACSHA512(passwordSalt))
//            {
//                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
//                return computedHash.SequenceEqual(passwordHash);
//            }
//        }

//        private string generateVerificationToken()
//        {
//            // token is a cryptographically strong random sequence of values
//            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

//            // ensure token is unique by checking against db
//            var tokenIsUnique = !_context.Users.Any(x => x.VerificationToken == token);
//            if (!tokenIsUnique)
//                return generateVerificationToken();

//            return token;
//        }

//        private void sendVerificationEmail(User user, string origin)
//        {
//            string message;
//            if (!string.IsNullOrEmpty(origin))
//            {
//                // origin exists if request sent from browser
//                // sending link to verify through browser
//                var verifyUrl = $"{origin}/account/verify-email?token={user.VerificationToken}";
//                message = $@"<p>Please click the below link to verify your email address:</p>
//                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
//            }
//            else
//            {
//                // origin missing if request sent directly to api with some restful app
//                // sending instructions to verify account through the api
//                message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
//                            <p><code>{user.VerificationToken}</code></p>";
//            }

//            _emailService.Send(
//                to: user.Email,
//                subject: "Sign-up Verification API - Verify Email",
//                html: $@"<h4>Verify Email</h4>
//                        <p>Thanks for registering!</p>
//                        {message}"
//            );
//        }
//        private void sendPasswordResetEmail(User user, string origin)
//        {
//            string message;
//            if (!string.IsNullOrEmpty(origin))
//            {
//                var resetUrl = $"{origin}/account/reset-password?token={user.ResetPasswordToken}";
//                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
//                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
//            }
//            else
//            {
//                message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
//                            <p><code>{user.ResetPasswordToken}</code></p>";
//            }

//            _emailService.Send(
//                to: user.Email,
//                subject: "Sign-up Verification API - Reset Password",
//                html: $@"<h4>Reset Password Email</h4>
//                        {message}"
//            );
//        }
//    }
//}
