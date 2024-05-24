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
        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
