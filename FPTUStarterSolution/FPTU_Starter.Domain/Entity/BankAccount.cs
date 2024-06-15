using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Domain.Entity
{
    public class BankAccount
    {
        public Guid Id { get; set; }
        public string? OwnerName { get; set; }
        public int BankAccountNumber { get; set; }
        public string? BankAccountName { get;}

    }
}
