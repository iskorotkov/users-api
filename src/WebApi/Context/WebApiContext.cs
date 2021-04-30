using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

namespace WebApi.Context
{
    public sealed class WebApiContext : DbContext
    {
        public WebApiContext(DbContextOptions<WebApiContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserState> UserStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
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

            modelBuilder.Entity<UserGroup>(entity => entity.ToTable("user_groups"));

            modelBuilder.Entity<UserState>(entity => entity.ToTable("user_states"));
        }
    }
}
