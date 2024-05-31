using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectImage;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO;
using static FPTU_Starter.Domain.Enum.ProjectEnum;

namespace FPTU_Starter.Application.ViewModel.ProjectDTO
{
    public class ProjectViewResponse
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal ProjectTarget { get; set; }
        public decimal ProjectBalance { get; set; }
        public string ProjectBankAccount { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        public Guid OwnerId { get; set; }
        public string ProjectOwnerName { get; set; } = string.Empty;

        public ProjectStatus ProjectStatus { get; set; }
        public List<PackageViewResponse> PackageViewResponses { get; set; }
        public List<ProjectImageViewResponse> StoryImages { get; set; }
    }
}
