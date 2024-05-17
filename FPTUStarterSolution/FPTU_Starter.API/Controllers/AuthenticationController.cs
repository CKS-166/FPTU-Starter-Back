using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("Login")]
        public async Task<ActionResult<ResponseToken>> login(LoginDTO loginDTO)
        {
            var result = await _authenticationService.LoginAsync(loginDTO);
            return Ok(result);
        }
        [HttpPost("Register")]
        public async Task<ActionResult<ResponseToken>> register(RegisterModel registerModel)
        {
            var result = await _authenticationService.RegisterUserAsync(registerModel); 
            return Ok(result);
        }
    }
}
