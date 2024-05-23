using FPTU_Starter.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO
{
    public class PackageAddRequest
    {
        public string PackageName { get; set; } = string.Empty;

        [Required]
        public int RequiredAmount { get; set; }
        [Required]
        public int LimitQuantity { get; set; }
        [Required]
        public string PackageType { get; set; } = string.Empty;
        [Required]
        public Guid ProjectId { get; set; }
    }
}
