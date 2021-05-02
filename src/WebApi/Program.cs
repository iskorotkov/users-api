using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models.Context;

namespace WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build()
                .ApplyMigrations();
            host.Run();
        }

        public static IHost ApplyMigrations(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebApiContext>();
            dbContext.Database.Migrate();
            return host;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
