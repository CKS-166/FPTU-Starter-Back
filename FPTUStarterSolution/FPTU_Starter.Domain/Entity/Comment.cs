using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Domain.Entity
{
    public class Comment
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public DateTime CreateDate { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
        //public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
