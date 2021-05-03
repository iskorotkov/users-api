using System;
using System.Threading.Tasks;
using Db.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApi
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build();
            await ApplyMigrations(host);
            await host.RunAsync();
        }

        private static async Task ApplyMigrations(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebApiContext>();

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    await dbContext.Database.MigrateAsync();
                    return;
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                }

                await Task.Delay(5000);
            }

            throw new ApplicationException("Couldn't initialize database");
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}
