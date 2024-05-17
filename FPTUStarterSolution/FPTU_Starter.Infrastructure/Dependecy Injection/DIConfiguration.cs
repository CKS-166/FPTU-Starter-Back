using FPTU_Starter.Application.IRepository;
using FPTU_Starter.Application.ITokenService;
using FPTU_Starter.Application.Services;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Domain;
using FPTU_Starter.Infrastructure.Authentication;
using FPTU_Starter.Infrastructure.Database;
using FPTU_Starter.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace FPTU_Starter.Infrastructure.Dependecy_Injection
{
    public static class DIConfiguration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddDbContext<MyDbContext>(option =>
            option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(DIConfiguration).Assembly.FullName)), ServiceLifetime.Scoped);           
            service.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            service.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(configuration["Jwt:key"]!))
                };
            });
            service.AddTransient<IUserRepository, UserRepository>();
            service.AddScoped<IAuthenticationService, AuthenticationService>();
            service.AddScoped<ITokenGenerator, TokenGenerator>();
            return service;
        }
    }
}
