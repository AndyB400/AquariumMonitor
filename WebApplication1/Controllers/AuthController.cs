using System;
using System.IdentityModel.Tokens.Jwt;
using AquariumMonitor.Models.APIModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AquariumAPI.Filters;
using AquariumMonitor.DAL.Interfaces;
using System.Threading.Tasks;
using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.BusinessLogic;

namespace AquariumAPI.Controllers
{
    [ValidateModel]
    [Produces("application/json")]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordManager _passwordManager;
        private readonly IAuthManager _authManager;

        public AuthController(IConfiguration configuration,
            ILogger<AuthController> logger,
            IUserRepository userRepository,
            IPasswordManager passwordManager, 
            IAuthManager authManager)
        {
            _configuration = configuration;
            _logger = logger;
            _userRepository = userRepository;
            _passwordManager = passwordManager;
            _authManager = authManager;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> CreateToken([FromBody] CredentialModel model)
        {
            try
            {
                var user = await _userRepository.Get(model.Username);

                if (user == null) return Unauthorized();
                if (! await _passwordManager.VerifyPassword(user.Id, model.Password)) return Unauthorized();

                var claims = await _authManager.GetAllClaims(user);

                var signingCredentials = _authManager.GetSigningCredentials();

                int validForDurationMinutes = 15;
                int.TryParse(_configuration[Constants.TokenDurationMinutes], out validForDurationMinutes);

                var token = _authManager.CreateToken(claims, validForDurationMinutes, signingCredentials);

                _logger.LogInformation($"Created Token. Username:{model.Username}");

                // Update User Last login time for audit
                await _userRepository.UpdateLastLogin(user.Id);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception thrown while creating JWT: {ex}");
            }
            return BadRequest("Failed to create token");
        }
    }
}