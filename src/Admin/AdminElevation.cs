using Models.Context;
using Models.Entities;

namespace Admin
{
    public class AdminElevation
    {
        private readonly WebApiContext _context;

        public AdminElevation(WebApiContext context)
        {
            _context = context;
        }

        public bool CanBecomeAdmin(User user)
        {
            return false;
        }
    }
}
