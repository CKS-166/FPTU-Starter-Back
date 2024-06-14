using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.ViewModel.WithdrawReqDTO
{
    public class WithdrawWalletResponse
    {
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }


    }
}
