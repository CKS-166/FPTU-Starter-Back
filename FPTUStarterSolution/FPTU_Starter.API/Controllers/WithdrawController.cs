﻿using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.WithdrawReqDTO;
using FPTU_Starter.Domain.Constrain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FPTU_Starter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WithdrawController : ControllerBase
    {
        private readonly IWithdrawService _withdrawService;

        public WithdrawController(IWithdrawService withdrawService)
        {
            _withdrawService = withdrawService;
        }
        // GET: api/<WithdrawController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _withdrawService.getAllRequest();
            return Ok(result);
        }

        // POST api/<WithdrawController>
        [HttpPost("create-project-request")]
        public async Task<IActionResult> CreateProjectRequest([FromBody] WithdrawRequestDTO requestDTO)
        {
            var result = await _withdrawService.createWithdrawRequest(requestDTO);
            if (!result._isSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("admin-project-request")]
        public async Task<IActionResult> AdminProjectRequest([FromBody] Guid id)
        {
            var result = await _withdrawService.processingProjectWithdrawRequest(id);
            if (!result._isSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = Role.Admin)]
        [HttpPost("admin-approved-project-request")]
        public async Task<IActionResult> ApprovedProjectRequest([FromBody] Guid id)
        {
            var result = await _withdrawService.approvedProjectWithdrawRequest(id);
            if (!result._isSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = Role.Backer)]
        [HttpPost("withdraw-wallet-request")]
        public async Task<IActionResult> WithdrawWalletRequest([FromBody] WithdrawWalletRequest request)
        {
            var result = await _withdrawService.WithdrawWalletRequest(request);
            if (!result._isSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = Role.Admin)]
        [HttpPost("admin-withdraw-wallet-request")]
        public async Task<IActionResult> WithdrawWalletRequest([FromForm] Guid requestId, Guid walletId)
        {
            var result = await _withdrawService.AdminApprovedWithdrawWalletRequest(requestId, walletId);
            if (!result._isSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}
