﻿using AutoMapper;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Application.ViewModel.UserDTO;
using FPTU_Starter.Domain.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IMapper _mapper;

        public UserManagementService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _mapper = mapper;
        }

        public async Task<bool> CheckIfUserExistByEmail(string email)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetAsync(x => x.Email == email);
                return user != null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<UserInfoResponse>> GetUserInfo()
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    return ResultDTO<UserInfoResponse>.Fail("User not authenticated.");
                }
                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    return ResultDTO<UserInfoResponse>.Fail("User not found.");
                }
                var userEmail = userEmailClaims.Value;
                var applicationUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (applicationUser == null)
                {
                    return ResultDTO<UserInfoResponse>.Fail("User not found.");
                }
                var userInfoResponse = _mapper.Map<UserInfoResponse>(applicationUser);

                return ResultDTO<UserInfoResponse>.Success(userInfoResponse);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<UserInfoResponse>> GetUserInfoById(Guid id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if(user is null)
                {
                    return ResultDTO<UserInfoResponse>.Fail("User not found.");
                }
                var userRes = _mapper.Map<UserInfoResponse>(user);
                return ResultDTO<UserInfoResponse>.Success(userRes);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<string>> UpdatePassword(string newPassword, string confirmPassword, string userEmail)
        {
            try
            {
                ApplicationUser applicationUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (applicationUser == null)
                {
                    return ResultDTO<string>.Fail("User not found.");
                }
                if (newPassword != confirmPassword)
                {
                    return ResultDTO<string>.Fail("Passwords do not match.");
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                var result = await _userManager.ResetPasswordAsync(applicationUser, token, newPassword);
                if (result.Succeeded)
                {
                    await _unitOfWork.CommitAsync();
                    return ResultDTO<string>.Success("Password updated successfully.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ResultDTO<string>.Fail($"Failed to update password: {errors}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<string>> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    return ResultDTO<string>.Fail("User not authenticated.");
                }
                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    return ResultDTO<string>.Fail("User not found.");
                }
                var userEmail = userEmailClaims.Value;
                ApplicationUser applicationUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                if (applicationUser == null)
                {
                    return ResultDTO<string>.Fail("User not found.");
                }
                _mapper.Map(userUpdateRequest, applicationUser);
                _unitOfWork.UserRepository.Update(applicationUser);
                await _unitOfWork.CommitAsync();

                return ResultDTO<string>.Success("Update Successfully");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
