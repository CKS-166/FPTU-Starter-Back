using AutoMapper;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.TransactionDTO;
using FPTU_Starter.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FPTU_Starter.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResultDTO<List<TransactionInfoResponse>>> GetAllTrans()
        {
            try
            {
                List<Transaction> trans = _unitOfWork.TransactionRepository.GetAll().ToList();

                var result = _mapper.Map<List<TransactionInfoResponse>>(trans);
                foreach ( var transaction in result)
                {
                    Wallet wallet = _unitOfWork.WalletRepository.GetQueryable().Include(w => w.Backer).FirstOrDefault(w => w.Id == transaction.WalletId);
                    transaction.BackerName = wallet.Backer.UserName;
                }
                return ResultDTO <List<TransactionInfoResponse>>.Success(result , "");
            }catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
