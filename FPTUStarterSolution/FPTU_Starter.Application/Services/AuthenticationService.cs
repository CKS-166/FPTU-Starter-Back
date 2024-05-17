
using FPTU_Starter.Application.ITokenService;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using FPTU_Starter.Domain.Constrain;
using FPTU_Starter.Domain.Entity;
using Microsoft.AspNetCore.Identity;
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

        public AuthenticationService(IUnitOfWork unitOfWork,
            ITokenGenerator tokenGenerator,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _tokenGenerator = tokenGenerator;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public async Task<ResultDTO<ResponseToken>> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var getUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == loginDTO.Email);

                if (getUser is null || !await _userManager.CheckPasswordAsync(getUser, loginDTO.Password))
                    return ResultDTO<ResponseToken>.Fail("Email or password is wrong");

                var userRole = await _userManager.GetRolesAsync(getUser);



                var token = _tokenGenerator.GenerateToken(getUser, userRole);
                return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = token }, "successfull create token");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
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
                    UserName = registerModel.Name,
                    Email = registerModel.Email,
                    Address = registerModel.Address,
                    Name = registerModel.Name,
                    Phone = registerModel.Phone,
                    Id = Guid.NewGuid(),
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
                var token = _tokenGenerator.GenerateToken(newUser,null);
                return ResultDTO<ResponseToken>.Success(new ResponseToken { Token = token }, "Successfully created user and token");
            }
            catch (Exception ex)
            {
                // Log the exception and return a failure result
                // Consider logging the exception to a file or monitoring system
                return ResultDTO<ResponseToken>.Fail($"An error occurred: {ex.Message}");
            }
        }

    }
}
