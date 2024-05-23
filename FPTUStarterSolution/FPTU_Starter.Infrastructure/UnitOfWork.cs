
using FPTU_Starter.Application;
using FPTU_Starter.Application.IRepository;
using FPTU_Starter.Infrastructure.Database;
using FPTU_Starter.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _dbContext;
        private IUserRepository _userRepository;
        private IProjectRepository _projectRepository;
        private IPackageRepository _packageRepository;
        private ICategoryRepository _categoryRepository;
        public UnitOfWork(MyDbContext dbContext, 
            IUserRepository UserRepository, 
            IProjectRepository projectRepository, 
            IPackageRepository packageRepository,
            ICategoryRepository categoryRepository)
        {
            _dbContext = dbContext;
            _userRepository = UserRepository;
            _projectRepository = projectRepository;
            _packageRepository = packageRepository;
            _categoryRepository = categoryRepository;
        }

        public IUserRepository UserRepository
        {
            get
            {
                return _userRepository = _userRepository ?? new UserRepository(_dbContext);
            }
        }

        public IProjectRepository ProjectRepository
        {
            get
            {
                return _projectRepository = _projectRepository ?? new ProjectRepository(_dbContext);
            }
        }

        public IPackageRepository PackageRepository
        {
            get
            {
                return _packageRepository = _packageRepository ?? new PackageRepository(_dbContext);
            }
        }
        public ICategoryRepository CategoryRepository
        {
            get
            {
                return _categoryRepository = _categoryRepository ?? new CategoryRepository(_dbContext);
            }
        }
        public void Commit()
             => _dbContext.SaveChanges();


        public async Task CommitAsync()
            => await _dbContext.SaveChangesAsync();


        public void Rollback()
            => _dbContext.Dispose();


        public async Task RollbackAsync()
            => await _dbContext.DisposeAsync();
    }
}

