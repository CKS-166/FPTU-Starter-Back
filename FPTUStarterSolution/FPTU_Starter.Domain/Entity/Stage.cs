using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Domain.Entity
{
    public class Stage
    {
        [Required]
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? StageName { get; set; }
        public bool IsFinish { get; set; }
        public string? StageDescription { get; set;}
        public Project? Projects { get; set; }
        public Guid ProjectID { get; set; }
    }
}
