using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Application.ViewModel;
using Microsoft.AspNetCore.Mvc;
using FPTU_Starter.Application;
using FPTU_Starter.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FPTU_Starter.Infrastructure.OuterService.Interface;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/[controller]")]
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

        [HttpPost("add-project")]
        public async Task<IActionResult> AddProject([FromForm] ProjectAddRequest projectAddRequest, [FromForm] IFormFile thumbnailFile, [FromForm] IFormFile liveDemoFile)
        {
            var thumbnailResult = await _photoService.UploadPhotoAsync(thumbnailFile);
            projectAddRequest.ProjectThumbnail = thumbnailResult.Url != null ? thumbnailResult.Url.ToString() : "" ;
            var videoResult = await _videoService.UploadVideoAsync(liveDemoFile);
            projectAddRequest.ProjectLiveDemo = videoResult.Url != null ? videoResult.Url.ToString() : "";
            var result = await _projectService.CreateProject(projectAddRequest);
            return Ok(result);
        }

        [HttpPost("add-thumbnail")]
        public async Task<IActionResult> UploadThumbnail(IFormFile file)
        {
            var result = await _photoService.UploadPhotoAsync(file);
            return Ok(result.Url);
        }

        [HttpPost("add-live-demo")]
        public async Task<IActionResult> UploadLiveDemo(IFormFile file)
        {
            var result = await _videoService.UploadVideoAsync(file);
            return Ok(result);
        }
    }
}
