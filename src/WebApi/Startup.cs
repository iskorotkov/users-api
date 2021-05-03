using System;
using System.Text.Json.Serialization;
using Admin;
using Auth.Hashing;
using Db.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Signup;
using WebApi.Mapper;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1" }); });

            var connStr = Configuration.GetConnectionString("WebApiContext");
            services.AddDbContext<WebApiContext>(builder => builder.UseSqlServer(connStr));

            services.AddAutoMapper(MapperExtensions.Configure);

            services.AddScoped<AdminElevation>();
            services.AddScoped<PasswordHasher>();
            services.AddScoped(provider =>
            {
                var context = provider.GetRequiredService<WebApiContext>();
                return new SignupThrottler(context, TimeSpan.FromSeconds(5));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi v1"));

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
