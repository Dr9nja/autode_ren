using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.WebAPI.Contracts;
using KooliProjekt.WebAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KooliProjekt.WebAPI.Controllers
{
    // <summary>Registers customers and manages their cookie-based sign-in session.</summary>
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>Registers and signs in a customer.</summary>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(AccountResponse), 201)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Register(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var account = await _accountService.RegisterAsync(request, cancellationToken);
            if (account == null)
            {
                return Conflict(new { message = "An account with this email already exists." });
            }

            await SignInAsync(account);
            return StatusCode(201, account);
        }

        /// <summary>Signs in a customer with their registered email and password.</summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var account = await _accountService.ValidateLoginAsync(request, cancellationToken);
            if (account == null)
            {
                return Unauthorized(new { message = "Email or password is incorrect." });
            }

            await SignInAsync(account);
            return Ok(account);
        }

        /// <summary>Returns the signed-in customer.</summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult Me()
        {
            var account = new AccountResponse(
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                User.FindFirstValue(ClaimTypes.Name),
                User.FindFirstValue(ClaimTypes.Email),
                User.IsInRole("Admin"));

            return Ok(account);
        }

        /// <summary>Ends the current customer session.</summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        private async Task SignInAsync(AccountResponse account)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Name),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.IsAdmin ? "Admin" : "Customer")
            };
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}
