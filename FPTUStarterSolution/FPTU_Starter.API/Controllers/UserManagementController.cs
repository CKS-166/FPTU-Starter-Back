using FPTU_Starter.Application;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.UserDTO;
using FPTU_Starter.Domain.Entity;
using FPTU_Starter.Infrastructure.OuterService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private IUserManagementService _userManagementService;
        private IPhotoService _photoService;
        public UserManagementController(IUserManagementService userManagementService, IPhotoService photoService)
        {
            _userManagementService = userManagementService;
            _photoService = photoService;
        }

        [HttpGet("user-profile")]
        [Authorize]
        public async Task<ActionResult> GetUserInformation()
        {
            var result = await _userManagementService.GetUserInfo();
            return Ok(result);
        }

        [HttpPut("user-profile")]
        [Authorize]
        public async Task<ActionResult> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            var result = await _userManagementService.UpdateUser(userUpdateRequest);
            return Ok(result);
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile imgFile)
        {
            var result = await _photoService.UploadPhotoAsync(imgFile);
            return Ok(result.Url);
        }
    }
}
