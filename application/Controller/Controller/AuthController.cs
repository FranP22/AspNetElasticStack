using Common.Service.Interface;
using Controller.Dto.Request;
using Database;
using Logging.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Security.Service.Interface;

namespace Controller.Controller
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _db;
        private readonly IAuthService _authService;
        private readonly IAppLoggerService<AuthController> _loggerService;
        private readonly IClientIpService _clientIpService;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AppDbContext db, IAuthService authService, IAppLoggerService<AuthController> loggerService, IClientIpService clientIpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _authService = authService;
            _loggerService = loggerService;
            _clientIpService = clientIpService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody]RegisterRequestDto request)
        {
            throw new NotImplementedException();
            var existingMail = await _userManager.FindByEmailAsync(request.Email);
            var existingUser = await _userManager.FindByNameAsync(request.Username);

            if (existingMail != null)
            {
                _loggerService.Warn("Register failed - email exists", new()
                {
                    Ip = _clientIpService.GetIp(),
                    UserId = existingMail.Id,
                    Service = "AuthController"
                });

                return BadRequest("Email already exists");
            }

            if (existingUser != null)
            {
                _loggerService.Warn("Register failed - username exists", new()
                {
                    Ip = _clientIpService.GetIp(),
                    UserId = existingUser.Id,
                    Service = "AuthController"
                });

                return BadRequest("User already exists");
            }

            var user = new IdentityUser
            {
                Email = request.Email,
                UserName = request.Username,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                _loggerService.Error("Register failed - Identity error", new()
                {
                    Ip = _clientIpService.GetIp(),
                    Service = "AuthController"
                });

                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "User");

            _loggerService.Info("User registered successfully", new()
            {
                Ip = _clientIpService.GetIp(),
                UserId = user.Id,
                Service = "AuthController"
            });
            return Ok(new
            {
                message = "User registered successfully"
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _loggerService.Warn("Login failed - user not found", new()
                {
                    Ip = _clientIpService.GetIp(),
                    Service = "AuthController"
                });

                return Unauthorized("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                _loggerService.Warn("Login failed - invalid password", new()
                {
                    Ip = _clientIpService.GetIp(),
                    UserId = user.Id,
                    Service = "AuthController"
                });

                return Unauthorized("Invalid credentials");
            }

            var accessToken = await _authService.GenerateTokenAsync(user);
            var refreshToken = await _authService.GenerateRefreshTokenAsync(user);

            _loggerService.Info("Login successful", new()
            {
                Ip = _clientIpService.GetIp(),
                UserId = user.Id,
                Service = "AuthController"
            });

            return Ok(new
            {
                accessToken = accessToken,
                refreshToken = refreshToken.Token
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody]string refreshToken)
        {
            await _authService.RevokeRefreshTokensAsync(refreshToken);

            _loggerService.Info("Logout successful", new()
            {
                Ip = _clientIpService.GetIp(),
                Service = "AuthController"
            });

            return Ok(new
            {
                message = "Logged out successfully"
            });
        }
    }
}
