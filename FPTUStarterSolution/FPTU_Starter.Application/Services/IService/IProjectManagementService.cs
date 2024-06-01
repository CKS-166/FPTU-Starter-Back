using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Domain.Entity;
using static FPTU_Starter.Domain.Enum.ProjectEnum;

namespace FPTU_Starter.Application.Services.IService
{
    public interface IProjectManagementService
    {
        Task<ResultDTO<string>> CreateProject(ProjectAddRequest projectAddRequest);
        Task<ResultDTO<List<ProjectViewResponse>>> ViewAllProjectsAsync();
        Task<ResultDTO<string>> UpdateProjectStatus(Guid id, ProjectStatus projectStatus);
        Task<ResultDTO<ProjectViewResponse>> GetProjectById(Guid id);
        Task<ResultDTO<List<ProjectViewResponse>>> GetUserProjects();
    }
}
