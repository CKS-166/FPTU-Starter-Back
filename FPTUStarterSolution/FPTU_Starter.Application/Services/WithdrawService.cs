using AutoMapper;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.WithdrawReqDTO;
using FPTU_Starter.Domain.Entity;
using FPTU_Starter.Domain.Enum;
using Microsoft.EntityFrameworkCore;
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
        private readonly ISystemWalletService _systemWalletService;
        private const int EXPIRED_DATE = 5;
        public WithdrawService(IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserManagementService userManagementService,
            IWalletService walletService,
            ISystemWalletService systemWalletService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManagementService = userManagementService;
            _walletService = walletService;
            _systemWalletService = systemWalletService;
        }

        public async Task<ResultDTO<WithdrawReqResponse>> createCashOutRequest(WithdrawRequestDTO requestDTO)
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
                if (!project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Successful)
                    && !project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Withdrawed))
                {
                    return ResultDTO<WithdrawReqResponse>.Fail("Project is not avaliable");
                }
                // Create new request
                WithdrawRequest request = new WithdrawRequest();
                request.Id = Guid.NewGuid();
                request.WalletId = userWallet._data.Id;
                request.IsFinished = false;

                request.ProjectId = project.Id;
                request.Status = Domain.Enum.WithdrawRequestStatus.Pending;
                request.CreatedDate = DateTime.UtcNow;
                request.ExpiredDate = request.CreatedDate.AddDays(EXPIRED_DATE);
                request.RequestType = Domain.Enum.TransactionTypes.CashOut;

                if (project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Successful))
                {
                    request.Amount = project.ProjectBalance;
                    project.ProjectStatus = ProjectEnum.ProjectStatus.Withdrawed;
                }
                else if (project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Withdrawed))
                {
                    request.Amount = project.ProjectBalance - project.ProjectTarget;
                }

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
                return ResultDTO<ProcessingWithdrawRequest>.Success(new ProcessingWithdrawRequest { projectBankAccount = project.BankAccount, BackerName = project.ProjectOwner?.AccountName }, "please transfer money into this bank account");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<List<DetailWithdraw>>> getAllRequest()
        {
            try
            {
                var withdrawRequests = await _unitOfWork.WithdrawRepository.GetAllAsync();
                var detailWithdrawList = new List<DetailWithdraw>();

                foreach (var withdrawRequest in withdrawRequests)
                {
                    // Assuming you have methods to get BankAccount and backerName based on withdrawRequest
                    var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(withdrawRequest.WalletId);
                    var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(wallet.BankAccountId);
                    var BackerName = await _unitOfWork.UserRepository.GetByIdAsync(wallet.BackerId);

                    var detailWithdraw = new DetailWithdraw
                    {
                        WithdrawRequest = withdrawRequest,
                        BankAccount = bankAccount,
                        backerName = BackerName.AccountName.ToString(),
                    };

                    detailWithdrawList.Add(detailWithdraw);
                   
                }
                return ResultDTO<List<DetailWithdraw>>.Success(detailWithdrawList, "List Request");
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
                //project.ProjectBalance -= request.Amount;
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

        public async Task<ResultDTO<WithdrawWalletResponse>> WithdrawWalletRequest(WithdrawWalletRequest request)
        {
            try
            {
                var user = await _userManagementService.GetUserInfo();
                ApplicationUser exitUser = _mapper.Map<ApplicationUser>(user._data);
                var userWallet = _unitOfWork.WalletRepository.Get(x => x.BackerId.Equals(exitUser.Id));

                // Check user
                if (user is null)
                {
                    return ResultDTO<WithdrawWalletResponse>.Fail("User is null");
                }

                // Check user wallet
                if (userWallet is null)
                {
                    return ResultDTO<WithdrawWalletResponse>.Fail("User wallet is null");
                }
                var isEnough = await _walletService.CheckAccoutBallance(request.Amount);
                if (!isEnough._data)
                {
                    return ResultDTO<WithdrawWalletResponse>.Fail("Amount not valid");
                }


                // Deduct amount from user wallet
                userWallet.Balance -= request.Amount;
                Wallet walletParse = _mapper.Map<Wallet>(userWallet);
                _unitOfWork.WalletRepository.Update(walletParse);

                // Create new Transaction
                Transaction transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.Now,
                    Description = $"{exitUser.Name} has just transferred {request.Amount} to ADMIN",
                    TotalAmount = request.Amount,
                    TransactionType = TransactionTypes.Withdraw,
                    WalletId = userWallet.Id,
                };
                await _unitOfWork.TransactionRepository.AddAsync(transaction);

                // Create new withdraw request
                WithdrawRequest newRequest = new WithdrawRequest
                {
                    Id = Guid.NewGuid(),
                    WalletId = userWallet.Id,
                    IsFinished = false,
                    Amount = request.Amount,
                    Status = Domain.Enum.WithdrawRequestStatus.Pending,
                    CreatedDate = DateTime.UtcNow,
                    ExpiredDate = DateTime.UtcNow.AddDays(EXPIRED_DATE),
                    RequestType = TransactionTypes.Withdraw,
                };
                await _unitOfWork.WithdrawRepository.AddAsync(newRequest);
                //bankAccount 
                BankAccount bank = _mapper.Map<BankAccount>(request.bankAccountRequest);
                bank.Id = userWallet.BankAccountId;
                _unitOfWork.BankAccountRepository.Update(bank);

                // Commit database
                await _unitOfWork.CommitAsync();
                WithdrawWalletResponse response = new WithdrawWalletResponse
                {
                    Amount = request.Amount,
                    WalletId = userWallet.Id,
                    BankAccount = bank,
                };
                return ResultDTO<WithdrawWalletResponse>.Success(response, "Successfully created withdraw request, please wait for admin approval.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        public async Task<ResultDTO<WithdrawWalletResponse>> AdminApprovedWithdrawWalletRequest(Guid requestId)
        {
            try
            {
                //get request withdrawRequest
                var request = await _unitOfWork.WithdrawRepository.GetByIdAsync(requestId);
                //check null 
                if (request == null)
                {
                    return ResultDTO<WithdrawWalletResponse>.Fail("wrong request !!!");
                }
                if (!request.Status.Equals(WithdrawRequestStatus.Processing))
                {
                    return ResultDTO<WithdrawWalletResponse>.Fail("request has not been processing !!");
                }
                if (request.IsFinished)
                {
                    return ResultDTO<WithdrawWalletResponse>.Fail("request has already done !!");
                }

                if (request.ExpiredDate < DateTime.Now)
                {
                    if (request.Status.Equals(WithdrawRequestStatus.Rejected))
                    {
                        return ResultDTO<WithdrawWalletResponse>.Fail("This Request is Rejected already!!");
                    }

                    //create new Transaction
                    Transaction TerminatedTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        CreateDate = DateTime.Now,
                        Description = $"Your request expried, transfer money {request.Amount} back to wallet {request.WalletId}",
                        TotalAmount = request.Amount,
                        TransactionType = TransactionTypes.Refund,
                        WalletId = request.WalletId,
                    };
                    request.Status = WithdrawRequestStatus.Rejected;
                    await _unitOfWork.CommitAsync();
                    return ResultDTO<WithdrawWalletResponse>.Fail("This Request is expired!!");
                }

                request.Status = WithdrawRequestStatus.Successful;
                request.IsFinished = true;

                //transaction 
                Transaction transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    Description = $"wallet {request.WalletId} has bean WithDraw with amount: {request.Amount}",
                    TotalAmount = request.Amount,
                    TransactionType = TransactionTypes.Withdraw,
                    WalletId = request.WalletId,
                };
                await _unitOfWork.TransactionRepository.AddAsync(transaction);
                //commit
                await _unitOfWork.CommitAsync();
                return ResultDTO<WithdrawWalletResponse>.Success(new WithdrawWalletResponse { WalletId = request.WalletId, Amount = request.Amount }, "Your request has been updated to successfull");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<WithdrawDetailResponse>> WithdrawRequestDetail(Guid RequestId)
        {
            try
            {
                //get request withdrawRequest
                var request = await _unitOfWork.WithdrawRepository.GetByIdAsync(RequestId);
                //check null 
                if (request == null)
                {
                    return ResultDTO<WithdrawDetailResponse>.Fail("wrong request !!!");
                }

                //check date expired
                if (request.ExpiredDate < DateTime.Now)
                {
                    return ResultDTO<WithdrawDetailResponse>.Fail("expired!!!");
                }
                //bank
                var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(request.WalletId);
                if (wallet is null)
                {
                    return ResultDTO<WithdrawDetailResponse>.Fail("Không tìm thấy ví");
                }
                var bankAcc = await _unitOfWork.BankAccountRepository.GetByIdAsync(wallet.BankAccountId);
                if (bankAcc is null)
                {
                    return ResultDTO<WithdrawDetailResponse>.Fail("Không tìm thấy Bank");
                }
                // backer name 
                var BackerName = await _unitOfWork.UserRepository.GetByIdAsync(wallet.BackerId);
                if (BackerName is null)
                {
                    return ResultDTO<WithdrawDetailResponse>.Fail("Không tìm thấy tên của Backer");
                }
                request.Status = WithdrawRequestStatus.Processing;
                await _unitOfWork.CommitAsync();
                return ResultDTO<WithdrawDetailResponse>.Success(new WithdrawDetailResponse { bankAcoount = bankAcc, BackerName = BackerName.AccountName }, "please transfer money into this bank account");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<WithdrawRequest>> RejectProcessingRequestWallet(Guid requestId)
        {
            try
            {
                //get request withdrawRequest
                var request = await _unitOfWork.WithdrawRepository.GetByIdAsync(requestId);
                //check null 
                if (request == null)
                {
                    return ResultDTO<WithdrawRequest>.Fail("wrong request !!!");
                }
                if (!request.Status.Equals(WithdrawRequestStatus.Processing) || !request.Status.Equals(WithdrawRequestStatus.Pending))
                {
                    return ResultDTO<WithdrawRequest>.Fail("request has not been processing !!");
                }
                if (request.IsFinished)
                {
                    return ResultDTO<WithdrawRequest>.Fail("request has already done !!");
                }

                if (request.Status.Equals(WithdrawRequestStatus.Rejected))
                {
                    return ResultDTO<WithdrawRequest>.Fail("This Request is Rejected already!!");
                }
                //TransferMoney
                var getWallet = await _unitOfWork.WalletRepository.GetAsync(x => x.Id.Equals(request.WalletId));
                if (getWallet == null)
                {
                    return ResultDTO<WithdrawRequest>.Fail("This Wallet is Null already!!");
                }
                getWallet.Balance += request.Amount;
                _unitOfWork.WalletRepository.Update(getWallet);

                //create new Transaction
                Transaction TerminatedTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.Now,
                    Description = $"Your request expried, transfer money {request.Amount} back to wallet {request.WalletId}",
                    TotalAmount = request.Amount,
                    TransactionType = TransactionTypes.Refund,
                    WalletId = request.WalletId,
                };

                //Rejected
                request.Status = WithdrawRequestStatus.Rejected;
                await _unitOfWork.TransactionRepository.AddAsync(TerminatedTransaction);

                //commit
                await _unitOfWork.CommitAsync();
                return ResultDTO<WithdrawRequest>.Success(new WithdrawRequest { WalletId = request.WalletId, Amount = request.Amount }, "Your request has been updated to Rejected");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<WithdrawRequest>> RejectProcessingRequestProject(Guid requestId)
        {
            try
            {
                //get request withdrawRequest
                var request = await _unitOfWork.WithdrawRepository.GetByIdAsync(requestId);
                //check null 
                if (request == null)
                {
                    return ResultDTO<WithdrawRequest>.Fail("wrong request !!!");
                }
                if (!request.Status.Equals(WithdrawRequestStatus.Processing) || !request.Status.Equals(WithdrawRequestStatus.Pending))
                {
                    return ResultDTO<WithdrawRequest>.Fail("request has not been processing !!");
                }
                if (request.IsFinished)
                {
                    return ResultDTO<WithdrawRequest>.Fail("request has already done !!");
                }

                if (request.Status.Equals(WithdrawRequestStatus.Rejected))
                {
                    return ResultDTO<WithdrawRequest>.Fail("This Request is Rejected already!!");
                }

                //TransferMoney
                var getWallet = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(request.ProjectId));
                if (getWallet == null)
                {
                    return ResultDTO<WithdrawRequest>.Fail("This Wallet is Null already!!");
                }
                getWallet.ProjectBalance += request.Amount;
                _unitOfWork.ProjectRepository.Update(getWallet);

                //create new Transaction
                Transaction TerminatedTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.Now,
                    Description = $"Your request expried, transfer money {request.Amount} back to Project {request.WalletId}",
                    TotalAmount = request.Amount,
                    TransactionType = TransactionTypes.Refund,
                    WalletId = request.WalletId,
                };

                //Rejected
                request.Status = WithdrawRequestStatus.Rejected;
                await _unitOfWork.TransactionRepository.AddAsync(TerminatedTransaction);

                //commit
                await _unitOfWork.CommitAsync();
                return ResultDTO<WithdrawRequest>.Success(new WithdrawRequest { WalletId = request.WalletId, Amount = request.Amount }, "Your request has been updated to Rejected");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
