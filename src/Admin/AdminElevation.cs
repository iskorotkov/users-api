using System.Threading.Tasks;
using Db.Context;
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

        public async Task<bool> CanBecomeAdmin(int? userId = null)
        {
            var admin = await _context.GetAdminAsync();
            return admin == null || admin.Id == userId;
        }

        public async Task<bool> CanEnterGroup(int groupId, int? userId = null)
        {
            var userGroup = await _context.UserGroups.FindAsync(groupId);
            return userGroup.Code == UserGroupCode.User ||
                   await CanBecomeAdmin(userId);
        }
    }
}
