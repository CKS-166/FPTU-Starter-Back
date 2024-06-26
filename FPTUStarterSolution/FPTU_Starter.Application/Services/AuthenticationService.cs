
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace FPTU_Starter.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private static Random random = new Random();
        private readonly IUserManagementService _userManagementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService.IEmailService _emailService;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IGoogleService _googleService;
        public AuthenticationService(IUnitOfWork unitOfWork,
            ITokenGenerator tokenGenerator,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService.IEmailService emailService,
            ILogger<AuthenticationService> logger,
            IGoogleService googleService ,
            IUserManagementService userManagementService)
        {
            _unitOfWork = unitOfWork;
            _tokenGenerator = tokenGenerator;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _logger = logger;
            _googleService = googleService;
            _userManagementService = userManagementService;
        }

        public async Task<ResultDTO<LoginResponseDTO>> GoogleLogin(string token)
        {
            try
            {
                var validPayload = await _googleService.VerifyGoogleTokenAsync(token);

                if (validPayload != null)
                {
                    // Check if the user already exists in your database based on email

                    var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == validPayload.Email);
                    //check if existingUser is exit in db 
                    if (existingUser == null)
                    {

                        var user = new ApplicationUser
                        {
                            AccountName = validPayload.Email,
                            Name = validPayload.GivenName + ' ' + validPayload.FamilyName,
                            UserName = validPayload.Email,
                            DayOfBirth = DateTime.Now,
                            Gender = Gender.Khác,
                            Email = validPayload.Email,
                            NormalizedEmail = validPayload.Email!.ToUpper(),
                            Address = "",
                            PhoneNumber = "",
                            TwoFactorEnabled = true, //enable 2FA
                        };


                        var result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            return ResultDTO<LoginResponseDTO>.Fail("invalid condition");
                        }
                        await _userManager.AddToRoleAsync(user, "User");
                        existingUser = user;

                    }

                    var jwtToken = _tokenGenerator.GenerateToken(existingUser, null);
                    var LoginRes = new LoginResponseDTO
                    {
                        AccessToken = jwtToken,
                        Expire = DateTime.Now.AddMinutes(60)
                    };

                    return ResultDTO<LoginResponseDTO>.Success(LoginRes);
                }

                return ResultDTO<LoginResponseDTO>.Fail("Invalid Google token");
            }
            catch (Exception ex)
            {
                return ResultDTO<LoginResponseDTO>.Fail("Server fail");
            }
        }

        public async Task<ResultDTO<ResponseToken>> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var getUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == loginDTO.Email);

                if (getUser is null || !await _userManager.CheckPasswordAsync(getUser, loginDTO.Password))
                    return ResultDTO<ResponseToken>.Fail("Email hoặc mật khẩu không đúng");

                var userRole = await _userManager.GetRolesAsync(getUser);
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

                // Create a new user
                var newUser = new ApplicationUser
                {
                    AccountName = registerModel.AccountName,
                    Name = registerModel.Name,
                    UserName = registerModel.Name,
                    Email = registerModel.Email,
                    Gender = null,
                    DayOfBirth = null,
                    NormalizedEmail = registerModel.Email,
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
                //Create new Wallet for every User sign in
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    Balance = 0,
                    Backer = newUser,
                    BackerId = newUser.Id,

                };
                await _unitOfWork.WalletRepository.AddAsync(wallet);

                // Optionally commit the changes if using a unit of work pattern
                await _unitOfWork.CommitAsync();
                // Generate a token for the new user
                if (newUser.TwoFactorEnabled)
                {
                    await _signInManager.SignOutAsync();
                    await _signInManager.PasswordSignInAsync(newUser, registerModel.Password, false, true);
                    var emailToken = await _userManager.GenerateTwoFactorTokenAsync(newUser, "Email");
                    var mess = new Message(new string[] { newUser.Email! }, "OTP Verification", emailToken);
                    _emailService.SendEmail(mess);
                    return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = $"OTP have been send to your email {newUser.Email}" });
                }
                var token = _tokenGenerator.GenerateToken(newUser, null);
                return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = token }, "Successfully created user and token");
            }
            catch (Exception ex)
            {

                return ResultDTO<ResponseToken>.Fail($"An error occurred: {ex.Message}");
            }
        }
        public async Task<ResultDTO<ResponseToken>> RegisterGoogleIdentity(RegisterModel registerModel, string role, string avatarUrl)
        {
            try
            {
                // Check if the user already exists
                var getUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == registerModel.Email);
                if (getUser != null)
                {
                    return ResultDTO<ResponseToken>.Fail("User already exists");
                }


                // Create a new user
                var newUser = new ApplicationUser
                {
                    AccountName = registerModel.AccountName,
                    Name = registerModel.Name,
                    UserName = registerModel.Name,
                    Email = registerModel.Email,
                    Gender = null,
                    DayOfBirth = null,
                    NormalizedEmail = registerModel.Email!.ToUpper(),
                    Avatar = avatarUrl,
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
                // Log the exception and return a failure result
                // Consider logging the exception to a file or monitoring system
                return ResultDTO<ResponseToken>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ResultDTO<string>> sendResetPasswordLink(string userEmail)
        {
            try
            {
                var getUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (getUser == null)
                {
                    return ResultDTO<string>.Fail("User not found.");
                }
                else
                {
                    string newPassword = GenerateRandomPassword(7);
                    var result = await _userManagementService.UpdatePassword(newPassword, newPassword, userEmail);
                    if (result._isSuccess == true)
                    {
                        string subject = "Đặt lại mật khẩu";
                        string body =
                        $"Chào {getUser.AccountName},\n\n" +
                        "Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu của bạn. Vui lòng nhập lại mật khẩu mới để đăng nhập vào hệ thống:\n\n" +
                        $"{newPassword}\n\n" +
                        "Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.\n\n" +
                        "Lưu ý: Nên đổi mật khẩu lại sau khi đăng nhập thành công.\n\n" +
                        "Trân trọng,\n" +
                        "FPTU Starter";
                        var mess = new Message(new string[] { getUser.Email! }, subject, body);
                        _emailService.SendEmail(mess);
                        return ResultDTO<string>.Success($"Reset Password link have been send to your email {getUser.Email}");
                    }
                    else
                    {
                        return ResultDTO<string>.Fail($"Lỗi xảy ra, vui lòng thử lại sau");
                    }
                }
            }catch (Exception ex)
            {
                return ResultDTO<string>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public static string GenerateRandomPassword(int length = 7)
        {
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string specialChars = "!@#$%^&*()_-+=<>?";

            if (length < 7)
            {
                throw new ArgumentException("Mật khẩu phải dài tối thiểu 7 kí tự");
            }

            var passwordChars = new StringBuilder();
            passwordChars.Append(upperCase[random.Next(upperCase.Length)]);
            passwordChars.Append(numbers[random.Next(numbers.Length)]);
            passwordChars.Append(specialChars[random.Next(specialChars.Length)]);

            string allChars = upperCase + lowerCase + numbers + specialChars;
            for (int i = passwordChars.Length; i < length; i++)
            {
                passwordChars.Append(allChars[random.Next(allChars.Length)]);
            }

            return new string(passwordChars.ToString().OrderBy(c => random.Next()).ToArray());
        }
    }
}
