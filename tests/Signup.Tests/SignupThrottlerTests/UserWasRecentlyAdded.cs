using System;
using System.Threading.Tasks;
using Db.Context;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Utils.Seeding;
using Xunit;

namespace Signup.Tests.SignupThrottlerTests
{
    public abstract class UserWasRecentlyAdded<T> where T : Seeder, new()
    {
        private readonly Seeder _seeder;

        protected UserWasRecentlyAdded()
        {
            _seeder = new T();
        }

        [Fact]
        public async Task AddUserWithAnotherLogin()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);

            var signupThrottler = new SignupThrottler(context, TimeSpan.FromHours(1));

            var existingUser = await context.Users.FirstAsync();
            existingUser.CreatedDate = DateTime.Now;
            await context.SaveChangesAsync();

            (await signupThrottler.IsSignupAllowed(existingUser.Login + "x")).ShouldBe(true);
        }

        [Fact]
        public async Task AddUserWithSameLogin()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);

            var signupThrottler = new SignupThrottler(context, TimeSpan.FromHours(1));

            var existingUser = await context.Users.FirstAsync();
            existingUser.CreatedDate = DateTime.Now;
            await context.SaveChangesAsync();

            (await signupThrottler.IsSignupAllowed(existingUser.Login)).ShouldBe(false);
        }

        [Fact]
        public async Task AddUserWithSameLoginAfterTimeout()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);

            var signupThrottler = new SignupThrottler(context, TimeSpan.FromHours(1));

            var existingUser = await context.Users.FirstAsync();
            existingUser.CreatedDate = DateTime.Now - TimeSpan.FromHours(2);
            await context.SaveChangesAsync();

            (await signupThrottler.IsSignupAllowed(existingUser.Login)).ShouldBe(true);
        }
    }

    public class SqliteUserWasRecentlyAdded : UserWasRecentlyAdded<SqliteSeeder>
    {
    }

    public class SqlServerUserWasRecentlyAdded : UserWasRecentlyAdded<SqlServerSeeder>
    {
    }
}
