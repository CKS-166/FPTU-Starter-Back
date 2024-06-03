﻿using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Domain.Entity
{
    public class Wallet
    {
        [Key]
        public Guid Id { get; set; }
        public decimal Balance { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<WithdrawRequest> WithdrawRequests { get; set; }

        public string? BackerId { get; set; }
        [ForeignKey("BackerId")]
        public ApplicationUser Backer { get; set; }
    }


}
