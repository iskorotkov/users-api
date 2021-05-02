using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Entities;
using Shouldly;
using Utils.Seeding;
using Xunit;

namespace Signup.Tests.SignupThrottlerTests
{
    public abstract class UserWasRecentlyAdded<T> where T : ISeeder, new()
    {
        private readonly ISeeder _seeder;

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

            var userToAdd = new User
            {
                Login = existingUser.Login + "x"
            };

            (await signupThrottler.IsSignupAllowed(userToAdd)).ShouldBe(true);
        }

        [Fact]
        public async Task AddUserWithSameLogin()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);

            var signupThrottler = new SignupThrottler(context, TimeSpan.FromHours(1));

            var existingUser = await context.Users.FirstAsync();
            existingUser.CreatedDate = DateTime.Now;
            await context.SaveChangesAsync();

            var user = new User
            {
                Login = existingUser.Login
            };

            (await signupThrottler.IsSignupAllowed(user)).ShouldBe(false);
        }

        [Fact]
        public async Task AddUserWithSameLoginAfterTimeout()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);

            var signupThrottler = new SignupThrottler(context, TimeSpan.FromHours(1));

            var existingUser = await context.Users.FirstAsync();
            existingUser.CreatedDate = DateTime.Now - TimeSpan.FromHours(2);
            await context.SaveChangesAsync();

            var user = new User
            {
                Login = existingUser.Login
            };

            (await signupThrottler.IsSignupAllowed(user)).ShouldBe(true);
        }
    }

    public class SqliteUserWasRecentlyAdded : UserWasRecentlyAdded<SqliteSeeder>
    {
    }

    public class SqlServerUserWasRecentlyAdded : UserWasRecentlyAdded<SqlServerSeeder>
    {
    }
}
