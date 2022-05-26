using FullRuns.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Core
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public void Register(RegisterRequest model, string origin)
        {
            // validate
            if (_context.Accounts.Any(x => x.Email == model.Email))
            {
                // send already registered error in email to prevent account enumeration
                sendAlreadyRegisteredEmail(model.Email, origin);
                return;
            }

            // map model to new account object
            var account = _mapper.Map<Account>(model);

            // first registered account is an admin
            var isFirstAccount = _context.Accounts.Count() == 0;
            account.Role = isFirstAccount ? Role.Admin : Role.User;
            account.Created = DateTime.UtcNow;
            account.VerificationToken = generateVerificationToken();

            // hash password
            account.PasswordHash = BCrypt.HashPassword(model.Password);

            // save account
            _context.Accounts.Add(account);
            _context.SaveChanges();

            // send email
            sendVerificationEmail(account, origin);
        }
    }
}
