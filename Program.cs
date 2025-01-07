using EduCraftAPI.Data;
using EduCraftAPI.Entities;
using EduCraftAPI.Entities.User;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EduCraftAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var allowedOrigins = builder.Configuration["AllowedOrigins"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigin",
                    builder =>
                    {
                        builder.WithOrigins(allowedOrigins!)
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                     
                    });
            });

            var authenticationSetting = new AuthenticationSettings();
            builder.Services.AddSingleton(authenticationSetting);

            builder.Services.AddAuthorization(option =>
            {
                option.AddPolicy("IsBlock", builder => builder.RequireClaim("IsBlocked", false.ToString()));

            });
            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = "Bearer";
                option.DefaultScheme = "Bearer";
                option.DefaultChallengeScheme = "Bearer";

            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authenticationSetting.JwtIssuer,
                    ValidAudience = authenticationSetting.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSetting.JwtKey)),
                };
            });
            builder.Configuration.GetSection("Authentication").Bind(authenticationSetting);
            
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IPresentationService, PresentationService>();
            builder.Services.AddScoped<IUserContextService, UserContextService>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<IGenerateService, GenerateService>();

            builder.Services.AddHttpContextAccessor();


            builder.Services.AddControllers();

            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddDbContext<DataDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("LocalDBConnection"));
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Configuration.AddJsonFile("secrets.appsettings.json", optional: true, reloadOnChange: true);

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                SeedData.Initialize(services);
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowMyOrigin");

            app.MapControllers();

            app.Run();
        }
    }
}
