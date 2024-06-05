using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectDonate;
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

        Task<ResultDTO<string>> UpdateProject(ProjectUpdateRequest request); 
        Task<ResultDTO<List<ProjectViewResponse>>> GetUserProjects(string? searchType, string? searchName, ProjectStatus? projectStatus, int? moneyTarget, string? categoryName);
        Task<ResultDTO<string>> UpdatePackages(Guid id, List<PackageViewResponse> req);
        Task<ResultDTO<ProjectDonateResponse>> DonateProject(ProjectDonateRequest request);
        Task<ResultDTO<ProjectDonateResponse>> PackageDonateProject(PackageDonateRequest request);
    }
}
