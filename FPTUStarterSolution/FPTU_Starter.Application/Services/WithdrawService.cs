using AutoMapper;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.WithdrawReqDTO;
using FPTU_Starter.Domain.Entity;
using FPTU_Starter.Domain.Enum;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services
{
    public class WithdrawService : IWithdrawService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserManagementService _userManagementService;
        private readonly IWalletService _walletService;
        private const int EXPIRED_DATE = 5;
        public WithdrawService(IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserManagementService userManagementService,
            IWalletService walletService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManagementService = userManagementService;
            _walletService = walletService;
        }

        public async Task<ResultDTO<WithdrawReqResponse>> createWithdrawRequest(WithdrawRequestDTO requestDTO)
        {
            try
            {
                var user = _userManagementService.GetUserInfo().Result;
                ApplicationUser exitUser = _mapper.Map<ApplicationUser>(user._data);
                var userWallet = await _walletService.GetUserWallet();
                var project = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(requestDTO.ProjectId));
                //check user
                if (user is null)
                {
                    return ResultDTO<WithdrawReqResponse>.Fail("User is null");
                }
                //check project
                if (project is null)
                {
                    return ResultDTO<WithdrawReqResponse>.Fail("Project cannot be found");
                }
                //check user wallet
                if (userWallet is null)
                {
                    return ResultDTO<WithdrawReqResponse>.Fail("User wallet is null");
                }
                //check project status 
                if (!project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Successful))
                {
                    return ResultDTO<WithdrawReqResponse>.Fail("Project is not avaliable");
                }

                // Create new request
                WithdrawRequest request = new WithdrawRequest();
                request.Id = Guid.NewGuid();
                request.WalletId = userWallet._data.Id;
                request.IsFinished = false;
                request.Amount = project.ProjectBalance;
                request.ProjectId = project.Id;
                request.Status = Domain.Enum.WithdrawRequestStatus.Pending;
                request.CreatedDate = DateTime.UtcNow;
                request.ExpiredDate = request.CreatedDate.AddDays(EXPIRED_DATE);
                request.RequestType = Domain.Enum.TransactionTypes.CashOut;

                _unitOfWork.WithdrawRepository.Add(request);
                //commit database
                await _unitOfWork.CommitAsync();

                WithdrawReqResponse response = _mapper.Map<WithdrawReqResponse>(request);
                return ResultDTO<WithdrawReqResponse>.Success(response, "successfully create withdraw request, please wait for admin to approved");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

        public async Task<ResultDTO<ProcessingWithdrawRequest>> processingProjectWithdrawRequest(Guid RequestId)
        {
            try
            {
                //get request withdrawRequest
                var request = await _unitOfWork.WithdrawRepository.GetByIdAsync(RequestId);
                //check null 
                if (request == null)
                {
                    return ResultDTO<ProcessingWithdrawRequest>.Fail("wrong request !!!");
                }
                //get project 
                var project = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(request.ProjectId));
                //check date expired
                if (request.ExpiredDate < DateTime.Now)
                {
                    return ResultDTO<ProcessingWithdrawRequest>.Fail("expired!!!");
                }
                request.Status = WithdrawRequestStatus.Processing;
                await _unitOfWork.CommitAsync();
                return ResultDTO<ProcessingWithdrawRequest>.Success(new ProcessingWithdrawRequest { projectBankAccount = project.ProjectBankAccount }, "please transfer money into this bank account");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<WithdrawRequest>> getAllRequest()
        {
            try
            {
                var list = await _unitOfWork.WithdrawRepository.GetAllAsync();
                return list.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<WithdrawRequest>> approvedProjectWithdrawRequest(Guid RequestId)
        {
            try
            {
                //get request withdrawRequest
                var request = await _unitOfWork.WithdrawRepository.GetByIdAsync(RequestId);
                //check null 
                if (request == null)
                {
                    return ResultDTO<WithdrawRequest>.Fail("wrong request !!!");
                }
                //get project 
                var project = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(request.ProjectId));
                if (request.IsFinished)
                {
                    return ResultDTO<WithdrawRequest>.Fail("request has already done !!");
                }
                if (!request.Status.Equals(WithdrawRequestStatus.Processing))
                {
                    return ResultDTO<WithdrawRequest>.Fail("request Failed: request status is processing");
                }
                project.ProjectBalance -= request.Amount;
                request.Status = WithdrawRequestStatus.Successful;
                request.IsFinished = true;

                //transaction 
                Transaction transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    Description = $"project {project.ProjectName} has bean CASH OUT with amount: {request.Amount}",
                    TotalAmount = request.Amount,
                    TransactionType = TransactionTypes.CashOut,
                    WalletId = request.WalletId,              
                };
                await _unitOfWork.TransactionRepository.AddAsync(transaction);
                //commit
                await _unitOfWork.CommitAsync();
                return ResultDTO<WithdrawRequest>.Success(request, "Your request has been updated to successfull");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
