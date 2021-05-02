using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models.Context;
using Models.Entities;
using WebApi;

namespace Utils.Seeding
{
    public class SqlServerSeeder : ISeeder
    {
        public DbContextOptions<WebApiContext> DbContextOptions { get; }

        public SqlServerSeeder()
        {
            var host = Program.CreateHostBuilder(new string[] { }).Build().ApplyMigrations();
            var scope = host.Services.CreateScope();
            DbContextOptions = scope.ServiceProvider.GetRequiredService<DbContextOptions<WebApiContext>>();
            Seed();
        }

        private void Seed()
        {
            var rand = new Random();

            using var context = new WebApiContext(DbContextOptions);

            context.Database.EnsureDeleted();
            context.Database.Migrate();

            var groups = context.UserGroups.Select(g => g.Id).ToList();
            var states = context.UserStates.Select(s => s.Id).ToList();

            var usersToAdd = new List<User>();
            for (var i = 0; i < 10; i++)
            {
                usersToAdd.Add(new User
                {
                    Login = rand.Next().ToString(),
                    PasswordHash = rand.Next().ToString(),
                    Salt = rand.Next().ToString(),
                    CreatedDate = DateTime.Now - TimeSpan.FromSeconds(rand.Next()),
                    GroupId = groups[rand.Next(groups.Count)],
                    StateId = states[rand.Next(states.Count)]
                });
            }

            context.AddRange(usersToAdd);
            context.SaveChanges();
        }
    }
}
