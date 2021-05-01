using Models.Context;

namespace Signup
{
    public class SignupThrottler
    {
        private readonly WebApiContext _context;

        public SignupThrottler(WebApiContext context)
        {
            _context = context;
        }

        public bool SignupAllowed()
        {
            return true;
        }
    }
}
