using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Domain.Entity
{
    public class WithdrawRequest
    {
        [Key]
        public Guid Id { get; set; }

        public decimal Amount { get; set; }
        public bool IsFinished { get; set; }
        public string RequestType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
