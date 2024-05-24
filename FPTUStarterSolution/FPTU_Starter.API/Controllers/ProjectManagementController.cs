using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Application.ViewModel;
using Microsoft.AspNetCore.Mvc;
using FPTU_Starter.Application;
using FPTU_Starter.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FPTU_Starter.Infrastructure.OuterService.Interface;
using FPTU_Starter.API.Exception;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectManagementController : ControllerBase
    {
        private IProjectManagementService _projectService;
        private IPhotoService _photoService;
        private IVideoService _videoService;

        public ProjectManagementController(IProjectManagementService projectService,IPhotoService photoService, IVideoService videoService)
        {
            _projectService = projectService;
            _photoService = photoService;
            _videoService = videoService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllProjects()
        {
            var result = await _projectService.ViewAllProjectsAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddProject(ProjectAddRequest projectAddRequest)
        {
            try
            {
                var result = await _projectService.CreateProject(projectAddRequest);
                return Ok(result);
            }catch(ExceptionError ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost("add-thumbnail")]
        public async Task<IActionResult> UploadThumbnail(IFormFile thumbnailFile)
        {
            var result = await _photoService.UploadPhotoAsync(thumbnailFile);
            return Ok(result.Url);
        }

        [HttpPost("add-live-demo")]
        public async Task<IActionResult> UploadLiveDemo(IFormFile liveDemoFile)
        {
            var result = await _videoService.UploadVideoAsync(liveDemoFile);
            return Ok(result.Url);
        }
    }
}
