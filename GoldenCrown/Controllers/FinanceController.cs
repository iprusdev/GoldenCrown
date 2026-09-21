using FluentValidation;
using GoldenCrown.Attributes;
using GoldenCrown.DTOs.Finance;
using GoldenCrown.Features.Finance.Deposit;
using GoldenCrown.Features.Finance.Transfer;
using GoldenCrown.Features.Finance.GetBalance;
using GoldenCrown.Features.Finance.GetTransactionHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [MyAuthorize]
    public class FinanceController : Controller
    {
        private readonly ISender _sender;
        public FinanceController (ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalanceAsync(CancellationToken cancellationToken)
        {
            var balanceResult = await _sender.Send(new GetBalanceQuery(GetUserid()), cancellationToken);
            if (!balanceResult.IsSuccess)
            {
                return BadRequest(new
                {
                    Message = balanceResult.ErrorMessage
                });

            }
            return Ok(new
            {
                Balance = balanceResult.Value
            });
        }
        [HttpPost("deposit")]
        public async Task<IActionResult> DepositAsync([FromBody] DepositRequest request, [FromServices] IValidator<DepositRequest> validator, CancellationToken cancellationToken) {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }
            var depositResult = await _sender.Send(new DepositCommand(GetUserid(), request.Amount), cancellationToken);
            if (depositResult.IsSuccess) {
                return Ok();
            }
            return BadRequest(new { depositResult.ErrorMessage });
        }
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferAsync([FromBody] TransferRequest request, [FromServices] IValidator<TransferRequest> validator, CancellationToken cancellationToken) {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }
            var transferResult = await _sender.Send(new TransferCommand(GetUserid(), request.ReceiverLogin, request.Amount), cancellationToken);
            if (transferResult.IsSuccess) {
                return Ok();
            }
            return BadRequest(new { transferResult.ErrorMessage });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistoryAsync([FromQuery] TransactionHistoryRequest request, [FromServices] IValidator<TransactionHistoryRequest> validator, CancellationToken cancellationToken)

        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }
            var historyResult = await _sender.Send(new GetTransactionHistoryQuery(GetUserid(), request.From, request.To, request.Offset, request.Limit), cancellationToken);
            if (historyResult.IsSuccess)
            {
                return Ok(historyResult.Value);
            }
            return BadRequest(new { Message = historyResult.ErrorMessage });
        }
        internal int GetUserid()
        {
           var userId = HttpContext.Items[Constants.UserIdContextParameter] as int?;
            return userId!.Value;
        }
    }
}
