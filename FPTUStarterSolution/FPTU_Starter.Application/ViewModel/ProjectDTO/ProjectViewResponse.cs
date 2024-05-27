using FPTU_Starter.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO;
using static FPTU_Starter.Domain.Enum.ProjectEnum;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectImage;

namespace FPTU_Starter.Application.ViewModel.ProjectDTO
{
    public class ProjectViewResponse
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
         public decimal ProjectTarget { get; set; }
        public decimal ProjectBalance { get; set; }
        public string ProjectBankAccount { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        public Guid OwnerId { get; set; }
        public string ProjectOwnerName {  get; set; } = string.Empty;  
        
        public ProjectStatus ProjectStatus { get; set; }
        public List<PackageViewResponse> PackageViewResponses { get; set; }
        public List<ProjectImageViewResponse> StoryImages { get; set; }
    }
}
