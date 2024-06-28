﻿using FPTU_Starter.Application.ViewModel.BankAccountDTO;
using FPTU_Starter.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.ViewModel.WithdrawReqDTO
{
    public class WithdrawWalletRequest
    {
        public decimal Amount { get; set; }
        public BankAccountRequest bankAccountRequest { get; set; }
    }
}
