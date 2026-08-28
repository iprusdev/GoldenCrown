using GoldenCrown.DTOs.Finance;
using GoldenCrown.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceController : Controller
    {
        private IFinanceService _financeService;
        public FinanceController (IFinanceService financeService)
        {
            _financeService = financeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBalanceAsync([FromHeader]string token)
        {
            var balanceResult = await _financeService.GetBalanceAsync(token);
            if (!balanceResult.IsSuccess)
            { 
                return BadRequest(new
                {
                    Message = "Session not found"
                });

            }
            return Ok(new
            {
                Balance = balanceResult.Value
            });
        }
        [HttpPost("deposit")]
        public async Task<IActionResult> DepositAsync([FromBody] DepositRequest request) {
            var depositResult = await _financeService.DepositAsync(request.Token, request.Amount);
            if (depositResult.IsSuccess) {
                return Ok();
            }
            return BadRequest(new { depositResult.ErrorMessage });
        }
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferAsync([FromBody] TransferRequest request) {
            var transferResult = await _financeService.TransferAsync(request.Token, request.ReceiverLogin, request.Amount);
            if (transferResult.IsSuccess) {
                return Ok();
            }
            return BadRequest(new { transferResult.ErrorMessage });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistoryAsync([FromQuery] TransactionHistoryRequest request)
        {
            var historyResult = await _financeService.GetHistoryAsync(request.Token, request.From, request.To, request.Offset, request.Limit);
            if (historyResult.IsSuccess)
            {
                return Ok(historyResult.Value);
            }
            return BadRequest(new { Message = historyResult.ErrorMessage });
        }
    }
}
