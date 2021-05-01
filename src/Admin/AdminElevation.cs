using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Entities;
using Models.Enums;

namespace Admin
{
    public class AdminElevation
    {
        private readonly WebApiContext _context;

        public AdminElevation(WebApiContext context)
        {
            _context = context;
        }

        public async Task<bool> CanBecomeAdmin(User user)
        {
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Group.Code == UserGroupCode.Admin);

            return admin == null || admin.Id == user.Id;
        }

        public async Task<bool> CanEnterGroup(User user)
        {
            var userGroup = await _context.UserGroups.FindAsync(user.GroupId);
            return userGroup.Code == UserGroupCode.User ||
                   await CanBecomeAdmin(user);
        }
    }
}
