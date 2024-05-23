using AutoMapper;
using FPTU_Starter.Application.IEmailService;
using FPTU_Starter.Application.IRepository;
using FPTU_Starter.Application.ITokenService;
using FPTU_Starter.Application.Services;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Domain;
using FPTU_Starter.Domain.EmailModel;
using FPTU_Starter.Domain.Entity;
using FPTU_Starter.Infrastructure.Authentication;
using FPTU_Starter.Infrastructure.CloudinaryClassSettings;
using FPTU_Starter.Infrastructure.Database;
using FPTU_Starter.Infrastructure.EmailService;
using FPTU_Starter.Infrastructure.MapperConfigs;
using FPTU_Starter.Infrastructure.OuterService.Implementation;
using FPTU_Starter.Infrastructure.OuterService.Interface;
using FPTU_Starter.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
            //DBContext
            service.AddDbContext<MyDbContext>(option =>
            option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(DIConfiguration).Assembly.FullName)), ServiceLifetime.Scoped);
            //BaseRepository          
            service.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            //Identity
            service.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MyDbContext>()
                .AddDefaultTokenProviders();


            //Authentication
            service.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
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


            //Email Configuration
            service.Configure<IdentityOptions>(
                opts => opts.SignIn.RequireConfirmedEmail = true
                );
            var emailCofig = configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfig>();
            service.AddSingleton(emailCofig);

            //CloudinarySetting
            service.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

            //Services and Repositories            
            service.AddTransient<IUserRepository, UserRepository>();
            service.AddScoped<IAuthenticationService, AuthenticationService>();
            service.AddScoped<ITokenGenerator, TokenGenerator>();
            service.AddScoped<IEmailService, EmailService.EmailService>();
            service.AddScoped<IProjectRepository, ProjectRepository>();
            service.AddScoped<IProjectManagementService, ProjectManagementService>();
            service.AddScoped<IPackageManagementService, PackageManagementService>();
            service.AddScoped<IPackageRepository, PackageRepository>();
            service.AddScoped<IPhotoService, UploadPhotoService>();
            service.AddScoped<IVideoService, UploadVideoService>();
            return service;
        }
    }
}
