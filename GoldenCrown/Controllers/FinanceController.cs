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
    }
}
