using FluentValidation;
using GoldenCrown.DTOs.User;
using GoldenCrown.Features.User.UserLogin;
using GoldenCrown.Features.User.UserRegister;
using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly ISender _sender;
        public UserController(ISender sender)
        {
            _sender = sender;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request, [FromServices]IValidator<RegisterRequest> validator, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }
            var result = await _sender.Send(new UserRegisterCommand(request.Login, request.Name, request.Password), cancellationToken);
            if (result)
            {
                return Ok();
            }
            return BadRequest(new { Message = result.ErrorMessage });

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request, [FromServices] IValidator<LoginRequest> validator, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }
            var result = await _sender.Send(new UserLoginCommand(request.Login, request.Password), cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(new {Token = result.Value});
            }
            return Unauthorized(new { Message = result.ErrorMessage });

        } 

    }
}
