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
using Microsoft.AspNetCore.Http;

namespace FPTU_Starter.Application.ViewModel.ProjectDTO
{
    public class ProjectAddRequest
    {

        public string ProjectName { get; set; } = string.Empty;

        public string ProjectDescription { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
         public decimal ProjectTarget { get; set; }

         public decimal ProjectBalance { get; set; }


        public string ProjectBankAccount { get; set; } = string.Empty;


        public ProjectStatus ProjectStatus { get; set; }


        public string ProjectThumbnail { get; set; } = string.Empty;

        public string ProjectLiveDemo { get; set; } = string.Empty;

 
        public Guid ProjectOwnerId { get; set; }

        public virtual ApplicationUser? ProjectOwner { get; set; }

        public List<PackageAddRequest> Packages { get; set; }
    }
}
