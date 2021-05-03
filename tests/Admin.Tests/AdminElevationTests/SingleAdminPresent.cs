using System.Linq;
using System.Threading.Tasks;
using Db.Context;
using Models.Entities;
using Shouldly;
using Utils.Seeding;
using Xunit;

namespace Admin.Tests.AdminElevationTests
{
    public abstract class SingleAdminPresent<T> where T : Seeder, new()
    {
        private readonly User _admin;
        private readonly UserGroup _adminGroup;
        private readonly Seeder _seeder;
        private readonly UserGroup _userGroup;

        protected SingleAdminPresent()
        {
            _seeder = new T();

            using var context = new WebApiContext(_seeder.DbContextOptions);

            _userGroup = context.GetUserGroup();
            _adminGroup = context.GetAdminGroup();

            _seeder.MakeAllActive();
            _admin = _seeder.MakeSingleAdmin();
            
            _admin.GroupId = _adminGroup.Id;
            context.SaveChanges();
        }

        [Fact]
        public async Task NewUserFailsToBecomeAdmin()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var adminElevation = new AdminElevation(context);

            var user = new User
            {
                Id = context.Users.Max(u => u.Id) + 1,
                GroupId = _adminGroup.Id
            };

            (await adminElevation.CanBecomeAdmin(user.Id)).ShouldBe(false);
            (await adminElevation.CanEnterGroup(user.GroupId, user.Id)).ShouldBe(false);
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

            (await adminElevation.CanBecomeAdmin(user.Id)).ShouldBe(false);
            (await adminElevation.CanEnterGroup(user.GroupId, user.Id)).ShouldBe(true);
        }

        [Fact]
        public async Task AdminBecomesAdminAgain()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var adminElevation = new AdminElevation(context);

            (await adminElevation.CanBecomeAdmin(_admin.Id)).ShouldBe(true);
            (await adminElevation.CanEnterGroup(_admin.GroupId, _admin.Id)).ShouldBe(true);
        }
    }

    public class SqlServerSingleAdminPresent : SingleAdminPresent<SqlServerSeeder>
    {
    }

    public class SqliteSingleAdminPresent : SingleAdminPresent<SqliteSeeder>
    {
    }
}
