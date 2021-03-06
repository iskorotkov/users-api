using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Models.Enums;

namespace Db.Context
{
    public sealed class WebApiContext : DbContext
    {
        public WebApiContext(DbContextOptions<WebApiContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserState> UserStates { get; set; }

        public IQueryable<User> GetActiveUsers() =>
            Users.Where(u => u.State.Code == UserStateCode.Active);

        public async Task<User> GetAdminAsync() => await GetActiveUsers()
            .FirstOrDefaultAsync(u => u.Group.Code == UserGroupCode.Admin);

        public async Task<UserGroup> GetAdminGroupAsync() =>
            await UserGroups.FirstOrDefaultAsync(g => g.Code == UserGroupCode.Admin);

        public async Task<UserGroup> GetUserGroupAsync() =>
            await UserGroups.FirstOrDefaultAsync(g => g.Code == UserGroupCode.User);

        public async Task<UserState> GetActiveStateAsync() =>
            await UserStates.FirstOrDefaultAsync(s => s.Code == UserStateCode.Active);

        public async Task<UserState> GetBlockedStateAsync() =>
            await UserStates.FirstOrDefaultAsync(s => s.Code == UserStateCode.Blocked);

        public User GetAdmin() => GetActiveUsers()
            .FirstOrDefault(u => u.Group.Code == UserGroupCode.Admin);

        public UserGroup GetAdminGroup() =>
            UserGroups.FirstOrDefault(g => g.Code == UserGroupCode.Admin);

        public UserGroup GetUserGroup() =>
            UserGroups.FirstOrDefault(g => g.Code == UserGroupCode.User);

        public UserState GetActiveState() =>
            UserStates.FirstOrDefault(s => s.Code == UserStateCode.Active);

        public UserState GetBlockedState() =>
            UserStates.FirstOrDefault(s => s.Code == UserStateCode.Blocked);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(u => u.Login).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Salt).IsRequired();
                entity.Property(u => u.CreatedDate).IsRequired();
                entity.Property(u => u.GroupId).IsRequired();
                entity.Property(u => u.StateId).IsRequired();

                entity.HasOne(u => u.Group)
                    .WithMany(g => g.Users)
                    .HasForeignKey(u => u.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("users_groups_fk");
                entity.HasOne(u => u.State)
                    .WithMany(s => s.Users)
                    .HasForeignKey(u => u.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("users_states_fk");
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("user_groups");

                entity.Property(g => g.Code).IsRequired();
                entity.Property(g => g.Description).IsRequired();
            });

            modelBuilder.Entity<UserState>(entity =>
            {
                entity.ToTable("user_states");

                entity.Property(s => s.Code).IsRequired();
                entity.Property(s => s.Description).IsRequired();
            });
        }
    }
}
