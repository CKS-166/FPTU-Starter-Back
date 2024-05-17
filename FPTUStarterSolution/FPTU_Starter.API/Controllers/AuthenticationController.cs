using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using FPTU_Starter.Domain.Constrain;
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
        [HttpPost("Register-Backer")]
        public async Task<ActionResult<ResponseToken>> registerBacker(RegisterModel registerModel)
        {
            var result = await _authenticationService.RegisterUserAsync(registerModel, Role.Backer);
            return Ok(result);
        }
        [HttpPost("Register-Admin")]
        public async Task<ActionResult<ResponseToken>> registerAdmin(RegisterModel registerModel)
        {
            var result = await _authenticationService.RegisterUserAsync(registerModel, Role.Admin);
            return Ok(result);
        }
        [HttpPost("Register-ProjectOwner")]
        public async Task<ActionResult<ResponseToken>> registerProjectOwner(RegisterModel registerModel)
        {
            var result = await _authenticationService.RegisterUserAsync(registerModel, Role.ProjectOwner);
            return Ok(result);
        }
    }
}
