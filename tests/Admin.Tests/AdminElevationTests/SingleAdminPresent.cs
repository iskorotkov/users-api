using System;
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
    public abstract class SingleAdminPresent<T> where T : ISeeder, new()
    {
        private readonly ISeeder _seeder;
        private readonly UserGroup _userGroup;
        private readonly UserGroup _adminGroup;
        private readonly User _admin;

        protected SingleAdminPresent()
        {
            _seeder = new T();

            using var context = new WebApiContext(_seeder.DbContextOptions);

            _userGroup = context.UserGroups.First(g => g.Code == UserGroupCode.User);
            _adminGroup = context.UserGroups.First(g => g.Code == UserGroupCode.Admin);

            var users = context.Users.ToList();
            foreach (var user in users)
            {
                user.GroupId = _userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            _admin = users[new Random().Next(users.Count)];
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
