using System;
using System.Collections.Generic;
using System.Linq;
using Db.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models.Entities;
using WebApi;

namespace Utils.Seeding
{
    public class SqlServerSeeder : Seeder, IDisposable
    {
        public sealed override DbContextOptions<WebApiContext> DbContextOptions { get; }
        private readonly IServiceScope _serviceScope;

        public SqlServerSeeder()
        {
            var host = Program.CreateHostBuilder(new string[] { }).Build();
            
            _serviceScope = host.Services.CreateScope();
            DbContextOptions = _serviceScope.ServiceProvider.GetRequiredService<DbContextOptions<WebApiContext>>();
            
            using var context = new WebApiContext(DbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.Migrate();

            Seed();
        }

        private void Seed()
        {
            var rand = new Random();

            using var context = new WebApiContext(DbContextOptions);

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

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }
    }
}
