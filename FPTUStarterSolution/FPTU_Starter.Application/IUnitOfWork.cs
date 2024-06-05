using FPTU_Starter.Application.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IProjectRepository ProjectRepository { get; }
        IPackageRepository PackageRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        ISubCategoryRepository SubCategoryRepository { get; }
        IWalletRepository WalletRepository { get; }

        IRewardItemRepository RewardItemRepository { get; }
        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
