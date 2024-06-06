using FPTU_Starter.Application.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/wallet")]
    [ApiController]
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserWallet()
        {
            var wallet = await _walletService.GetUserWallet();
            return Ok(wallet);

        }
    }
}
