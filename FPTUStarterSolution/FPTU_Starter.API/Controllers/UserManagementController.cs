using FPTU_Starter.Application;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private IUserManagementService _userManagementService;
        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("user-profile")]
        [Authorize]
        public async Task<ActionResult> GetUserInformation()
        {
            var result = await _userManagementService.GetUserInfo();
            return Ok(result);
        }
    }
}
