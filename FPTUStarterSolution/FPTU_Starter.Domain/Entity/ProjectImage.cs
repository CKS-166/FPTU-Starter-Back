using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Domain.Entity
{
    public class ProjectImage
    {
        public Guid Id { get; set; }
        public string? Url { get; set; }

        public Project? Project { get; set; }

    }
}
