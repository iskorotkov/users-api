using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Entities;
using Models.Enums;
using Shouldly;
using Utils.Seeding;
using Xunit;

namespace Admin.Tests.AdminElevationTests
{
    public abstract class NoAdminsPresent<T> where T : ISeeder, new()
    {
        private readonly ISeeder _seeder;
        private readonly UserGroup _userGroup;
        private readonly UserGroup _adminGroup;

        protected NoAdminsPresent()
        {
            _seeder = new T();

            using var context = new WebApiContext(_seeder.DbContextOptions);

            _userGroup = context.UserGroups.First(g => g.Code == UserGroupCode.User);
            _adminGroup = context.UserGroups.First(g => g.Code == UserGroupCode.Admin);

            foreach (var user in context.Users)
            {
                user.GroupId = _userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            context.SaveChanges();
        }

        [Fact]
        public async Task NewUserBecomesAdmin()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var adminElevation = new AdminElevation(context);

            var user = new User
            {
                Id = context.Users.Max(u => u.Id) + 1,
                GroupId = _adminGroup.Id
            };

            (await adminElevation.CanBecomeAdmin(user.Id)).ShouldBe(true);
            (await adminElevation.CanEnterGroup(user.GroupId, user.Id)).ShouldBe(true);
        }

        [Fact]
        public async Task NewUserBecomesPlainUser()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var adminElevation = new AdminElevation(context);

            var user = new User
            {
                Id = context.Users.Max(u => u.Id) + 1,
                GroupId = _userGroup.Id
            };

            (await adminElevation.CanBecomeAdmin(user.Id)).ShouldBe(true);
            (await adminElevation.CanEnterGroup(user.GroupId, user.Id)).ShouldBe(true);
        }
    }

    public class SqlServerNoAdminsPresent : NoAdminsPresent<SqlServerSeeder>
    {
    }

    public class SqliteNoAdminsPresent : NoAdminsPresent<SqliteSeeder>
    {
    }
}
