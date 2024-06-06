using AutoMapper;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.UserDTO;
using FPTU_Starter.Application.ViewModel.WalletDTO;
using FPTU_Starter.Domain.Entity;
using Google.Apis.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementService _userManagement;
        private readonly IMapper _mapper;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private const decimal MINIMUM_AMOUNT = 5000;

        public WalletService(IUnitOfWork unitOfWork, IUserManagementService userManagement, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManagement = userManagement;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _mapper = mapper;
        }

        public async Task<ResultDTO<bool>> CheckAccoutBallance(decimal amount)
        {
            try
            {
                // checking user exits
                var user = await _userManagement.GetUserInfo();
                if (user == null)
                {
                    return ResultDTO<bool>.Fail("Not found User");
                }

                //check amount MINIMUM
                if (amount < MINIMUM_AMOUNT)
                {
                    return ResultDTO<bool>.Fail("The minium amount is 5000vnd ");
                }

                //check amount divided 1000
                if (amount % 1000 != 0 )
                {
                    return ResultDTO<bool>.Fail("You can only donate amount divided to 1000");
                }
                //check wallet exits
                ApplicationUser exitUser = _mapper.Map<ApplicationUser>(user._data);
                var userWallet = await _unitOfWork.WalletRepository.GetAsync(x => x.BackerId!.Equals(exitUser.Id));
                if (userWallet == null)
                {
                    return ResultDTO<bool>.Fail("Not found any wallet match with this user");
                }
               
                //check balance
                if (userWallet.Balance > amount)
                {
                    return ResultDTO<bool>.Success(true, "your wallet is have enough money to do this transaction");
                }
                return ResultDTO<bool>.Fail("your wallet do not have enough money to do this transaction");


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<WalletResponse>> GetUserWallet()
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    return ResultDTO<WalletResponse>.Fail("User not authenticated.");
                }
                var userIdClaim = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return ResultDTO<WalletResponse>.Fail("User not found.");
                }
                var wallet = await _unitOfWork.WalletRepository.GetAsync(x => x.BackerId.Equals(userIdClaim.Value));
                var walletDTO = _mapper.Map<WalletResponse>(wallet);
                return ResultDTO<WalletResponse>.Success(walletDTO);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
