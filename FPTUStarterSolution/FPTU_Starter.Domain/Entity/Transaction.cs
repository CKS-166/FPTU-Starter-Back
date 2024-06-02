using System.ComponentModel.DataAnnotations;

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
        public decimal TotalAmount { get; set; }
        public string TransactionType { get; set; }
        public DateTime CreateDate { get; set; }

        public Guid SystemWalletId { get; set; }
        public SystemWallet SystemWallet { get; set; }
    }
}