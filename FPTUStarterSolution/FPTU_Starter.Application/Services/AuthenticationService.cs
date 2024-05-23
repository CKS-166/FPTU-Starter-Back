
using FPTU_Starter.Application.IEmailService;
using FPTU_Starter.Application.ITokenService;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using FPTU_Starter.Domain.Constrain;
using FPTU_Starter.Domain.EmailModel;
using FPTU_Starter.Domain.Entity;
using FPTU_Starter.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace FPTU_Starter.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService.IEmailService _emailService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IUnitOfWork unitOfWork,
            ITokenGenerator tokenGenerator,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService.IEmailService emailService,
            ILogger<AuthenticationService> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenGenerator = tokenGenerator;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _logger = logger;
        }
        public async Task<ResultDTO<ResponseToken>> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var getUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == loginDTO.Email);

                if (getUser is null || !await _userManager.CheckPasswordAsync(getUser, loginDTO.Password))
                    return ResultDTO<ResponseToken>.Fail("Email or password is wrong");

                var userRole = await _userManager.GetRolesAsync(getUser);
                if (getUser.TwoFactorEnabled)
                {
                    await _signInManager.SignOutAsync();
                    await _signInManager.PasswordSignInAsync(getUser, loginDTO.Password, false, true);
                    var emailToken = await _userManager.GenerateTwoFactorTokenAsync(getUser, "Email");
                    var mess = new Message(new string[] { getUser.Email! }, "OTP Verification", emailToken);
                    _emailService.SendEmail(mess);
                    return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = $"OTP have been send to your email {getUser.Email}" });
                }
                var token = _tokenGenerator.GenerateToken(getUser, userRole);
                return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = token }, "successfull create token");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<ResponseToken>> LoginWithOTPAsync(string code, string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    return ResultDTO<ResponseToken>.Fail("Can Not Found User !!!");
                }

                _logger.LogInformation($"Attempting 2FA sign-in for user {username} with code {code}.");
                var signIn = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", code);

                if (signIn)
                {
                    var userRole = await _userManager.GetRolesAsync(user);
                    var token = _tokenGenerator.GenerateToken(user, userRole);
                    return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = token }, "Successfully created token");
                }
                
                _logger.LogWarning($"Invalid code provided for user {username}.");
                return ResultDTO<ResponseToken>.Fail("Invalid Code !!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during 2FA login.");
                throw new Exception("An error occurred during 2FA login.", ex);
            }

        }

        public async Task<ResultDTO<ResponseToken>> RegisterUserAsync(RegisterModel registerModel, string role)
        {
            try
            {
                // Check if the user already exists
                var getUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == registerModel.Email);
                if (getUser != null)
                {
                    return ResultDTO<ResponseToken>.Fail("User already exists");
                }
                if (!Enum.TryParse<Gender>(registerModel.Gender, true, out var gender))
                {
                    throw new ArgumentException("Invalid gender value");
                }

                // Create a new user
                var newUser = new ApplicationUser
                {
                    AccountName = registerModel.AccountName,
                    Name = registerModel.Name,                    
                    UserName = registerModel.Name,
                    DayOfBirth = registerModel.DayOfBirth,
                    Gender = gender,
                    Email = registerModel.Email,
                    NormalizedEmail = registerModel.Email!.ToUpper(),
                    Address = registerModel.Address,
                    PhoneNumber = registerModel.Phone,
                    Id = Guid.NewGuid(),
                    TwoFactorEnabled = true, //enable 2FA
                   
                };

                // Add the user using UserManager
                var result = await _userManager.CreateAsync(newUser, registerModel.Password);
                if (!result.Succeeded)
                {
                    // Handle and log errors if user creation failed
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ResultDTO<ResponseToken>.Fail($"User creation failed: {errors}");
                }
                else
                {
                    //config role BACKER
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                    await _userManager.AddToRoleAsync(newUser, role);
                }

                // Optionally commit the changes if using a unit of work pattern
                await _unitOfWork.CommitAsync();
                // Generate a token for the new user
                var token = _tokenGenerator.GenerateToken(newUser, null);
                return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = token }, "Successfully created user and token");
            }
            catch (Exception ex)
            {

                return ResultDTO<ResponseToken>.Fail($"An error occurred: {ex.Message}");
            }
        }

    }
}
