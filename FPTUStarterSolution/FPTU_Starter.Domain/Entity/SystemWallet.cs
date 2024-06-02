using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Domain.Entity
{
    public class SystemWallet
    {
        [Key]
        public Guid Id { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime CreateDate { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
    }
}
