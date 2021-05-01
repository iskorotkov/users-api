using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Entities;

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

        public async Task<bool> IsSignupAllowed(User user)
        {
            var now = DateTime.Now;
            var loginRecentlyUsed = await _context.Users
                .Where(u => u.Login == user.Login)
                .AnyAsync(u => u.CreatedDate + _signupTimeout >= now);

            return !loginRecentlyUsed;
        }
    }
}
