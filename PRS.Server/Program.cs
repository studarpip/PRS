
using Microsoft.AspNetCore.Authentication.Cookies;
using Neo4j.Driver;
using PRS.Server.Helpers.Interfaces;
using PRS.Server.Helpers;
using PRS.Server.Migrations;
using PRS.Server.Migrations.Seeders;

namespace PRS.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var neo4jSection = builder.Configuration.GetSection("Neo4j");
            var uri = neo4jSection.GetValue<string>("Uri");
            var user = neo4jSection.GetValue<string>("Username");
            var pass = neo4jSection.GetValue<string>("Password");

            builder.Services.AddSingleton(GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass)));

            builder.Services.AddCors();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.SlidingExpiration = true;

                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IDatabaseSeeder, GenderSeeder>();
            builder.Services.AddScoped<IDatabaseSeeder, CountrySeeder>();
            builder.Services.AddScoped<SeederRunner>();
            builder.Services.AddSingleton<IEncryptionHelper, EncryptionHelper>();


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var seederRunner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
                await seederRunner.RunAllAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(policy => policy
                .WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
