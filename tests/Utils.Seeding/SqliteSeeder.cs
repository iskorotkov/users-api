using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Entities;
using Models.Enums;

namespace Utils.Seeding
{
    public class SqliteSeeder : ISeeder, IDisposable
    {
        private readonly DbConnection _connection;

        public SqliteSeeder()
        {
            _connection = CreateInMemoryDatabase();
            DbContextOptions = new DbContextOptionsBuilder<WebApiContext>()
                .UseSqlite(_connection)
                .Options;

            Seed();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public DbContextOptions<WebApiContext> DbContextOptions { get; }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        private void Seed()
        {
            var rand = new Random();

            using var context = new WebApiContext(DbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.UserGroups.AddRange(new UserGroup
            {
                Code = UserGroupCode.Admin,
                Description = rand.Next().ToString()
            }, new UserGroup
            {
                Code = UserGroupCode.User,
                Description = rand.Next().ToString()
            });

            context.UserStates.AddRange(new UserState
            {
                Code = UserStateCode.Active,
                Description = rand.Next().ToString()
            }, new UserState
            {
                Code = UserStateCode.Blocked,
                Description = rand.Next().ToString()
            });

            context.SaveChanges();

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
