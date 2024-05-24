using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FPTU_Starter.Domain.Enum.ProjectEnum;

namespace FPTU_Starter.Domain.Entity
{
    public class Project
    {
        public Project() {
            Packages = new HashSet<ProjectPackage>();
        }
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ProjectName { get; set; } = string.Empty;
        [Required]
        public string ProjectDescription { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        [Required] public decimal ProjectTarget{ get; set; }

        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        [Required] public decimal ProjectBalance { get; set; }

        [Required]
        public string ProjectBankAccount { get; set; } = string.Empty;

        [Required]
        public string ProjectThumbnail { get; set; } = string.Empty;

        [Required]
        public string ProjectLiveDemo { get; set; } = string.Empty;

        [Required]
        public ProjectStatus ProjectStatus { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public virtual ApplicationUser? ProjectOwner { get; set; }

        public virtual Category? Category { get; set; }

        public ICollection<ProjectPackage> Packages { get; set; }

    }
}
