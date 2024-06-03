using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPTU_Starter.Domain.Entity
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; }

        public Guid PackageId { get; set; }
        public string Description { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        public string TransactionType { get; set; }
        public DateTime CreateDate { get; set; }

        public Guid SystemWalletId { get; set; }
        public SystemWallet SystemWallet { get; set; }
    }
}