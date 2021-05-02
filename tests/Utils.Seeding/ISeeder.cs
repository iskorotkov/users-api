using Microsoft.EntityFrameworkCore;
using Models.Context;

namespace Utils.Seeding
{
    public interface ISeeder
    {
        public DbContextOptions<WebApiContext> DbContextOptions { get; }
    }
}
