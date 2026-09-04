using GoldenCrown.DTOs.User;
using GoldenCrown.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _userService.RegisterAsync(request.Login, request.Name, request.Password);
            if (result)
            {
                return Ok();
            }
            return BadRequest(new { Message = result.ErrorMessage });

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _userService.LoginAsync(request.Login, request.Password);
            if (result.IsSuccess)
            {
                return Ok(new {Token = result.Value});
            }
            return Unauthorized(new { Message = result.ErrorMessage });

        } 

    }
}
