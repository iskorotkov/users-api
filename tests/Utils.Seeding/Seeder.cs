using System;
using System.Linq;
using Db.Context;
using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Utils.Seeding
{
    public abstract class Seeder
    {
        public abstract DbContextOptions<WebApiContext> DbContextOptions { get; }

        public User MakeSingleAdmin()
        {
            using var context = new WebApiContext(DbContextOptions);

            MakeNoAdmins();

            var users = context.GetActiveUsers().ToList();
            var admin = users[new Random().Next(users.Count)];
            admin.GroupId = context.GetAdminGroup().Id;

            context.SaveChanges();

            return admin;
        }

        public void MakeNoAdmins()
        {
            using var context = new WebApiContext(DbContextOptions);
            
            var userGroup = context.GetUserGroup();
            foreach (var user in context.Users)
            {
                user.GroupId = userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            context.SaveChanges();
        }

        public void MakeAllActive()
        {
            using var context = new WebApiContext(DbContextOptions);
            var activeState = context.GetActiveState();
            foreach (var user in context.Users)
            {
                user.StateId = activeState.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            context.SaveChanges();
        }
    }
}
