
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

        public UnitOfWork(MyDbContext dbContext, IUserRepository UserRepository)
        {
            _dbContext = dbContext;
            _userRepository = UserRepository;
        }

        public IUserRepository UserRepository
        {
            get
            {
                return _userRepository = _userRepository ?? new UserRepository(_dbContext);
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

