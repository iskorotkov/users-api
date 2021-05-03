using System;
using System.Linq;
using System.Threading.Tasks;
using Db.Context;
using Microsoft.EntityFrameworkCore;

namespace Signup
{
    public class SignupThrottler
    {
        private readonly WebApiContext _context;
        private readonly TimeSpan _signupTimeout;

        public SignupThrottler(WebApiContext context, TimeSpan signupTimeout)
        {
            _context = context;
            _signupTimeout = signupTimeout;
        }

        public async Task<bool> IsSignupAllowed(string login)
        {
            var checkpoint = DateTime.Now - _signupTimeout;
            var loginRecentlyUsed = await _context.Users
                .Where(u => u.Login == login)
                .AnyAsync(u => u.CreatedDate >= checkpoint);

            return !loginRecentlyUsed;
        }
    }
}
