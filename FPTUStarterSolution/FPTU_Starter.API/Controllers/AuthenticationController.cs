using FPTU_Starter.Application.IEmailService;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using FPTU_Starter.Domain.Constrain;
using FPTU_Starter.Domain.EmailModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly Application.Services.IService.IAuthenticationService _authenticationService;
        private readonly IEmailService _emailService;

        public AuthenticationController(Application.Services.IService.IAuthenticationService authenticationService, IEmailService emailService)
        {
            _authenticationService = authenticationService;
            _emailService = emailService;
        }
        [HttpPost("login")]
        public async Task<ActionResult<ResponseToken>> login(LoginDTO loginDTO)
        {
            var result = await _authenticationService.LoginAsync(loginDTO);
            return Ok(result);
        }
        [HttpPost("register-backer")]
        public async Task<ActionResult<ResponseToken>> registerBacker(RegisterModel registerModel)
        {
            var result = await _authenticationService.RegisterUserAsync(registerModel, Role.Backer);
            return Ok(result);
        }
        [HttpPost("register-admin")]
        public async Task<ActionResult<ResponseToken>> registerAdmin(RegisterModel registerModel)
        {
            var result = await _authenticationService.RegisterUserAsync(registerModel, Role.Admin);
            return Ok(result);
        }
        [HttpPost("register-projectOwner")]
        public async Task<ActionResult<ResponseToken>> registerProjectOwner(RegisterModel registerModel)
        {
            var result = await _authenticationService.RegisterUserAsync(registerModel, Role.ProjectOwner);
            return Ok(result);
        }
        [HttpGet("test-send-email")]
        public async Task<IActionResult> TestEmail()
        {
            var mess = new Message(new string[] {
                "Maxcolasvn@gmail.com" }, "test", "<h1>Woah this is works !!</h1>");
            _emailService.SendEmail(mess);
            return Ok("The mess has send to your email, plz check ...");

        }
        [HttpPost]
        [Route("login-2FA")]
        public async Task<IActionResult> LoginWithOTP(string code, string username)
        {
           var result = await _authenticationService.LoginWithOTPAsync(code, username);
            if (!result._isSuccess)
            {
                return BadRequest();
            }
            return Ok(result);
        }

        [HttpPost("check-user-exist")]
        public async Task<ActionResult> CheckUserExistByEmail(string email)
        {
            var result = await _userManagementService.CheckIfUserExist(email);
            return Ok(result);
        }

        [HttpGet("signin-google")]
        public IActionResult SignInGoogle()
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Authentication");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return BadRequest();

            var claims = result.Principal.Identities
                .FirstOrDefault()?.Claims
                .Select(claim => new
                {
                    claim.Type,
                    claim.Value
                });

            return Ok(claims);
        }
    }
}
