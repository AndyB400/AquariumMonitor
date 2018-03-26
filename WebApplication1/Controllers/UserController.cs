using System;
using AquariumMonitor.Models.APIModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AquariumMonitor.Models;
using AquariumAPI.Filters;
using AquariumMonitor.DAL.Interfaces;
using System.Threading.Tasks;
using AutoMapper;
using AquariumMonitor.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using Pwned;

namespace AquariumAPI.Controllers
{
    [ValidateModel]
    [UserSecurityCheck]
    [Produces("application/json")]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _repository;
        private readonly IPasswordManager _passwordManager;
        private readonly IPasswordRepository _passwordRepository;
        private readonly IHaveIBeenPwnedRestClient _pwnedClient;

        public UserController(IConfiguration configuration,
            ILogger<UserController> logger,
            IUserRepository repository,
            IPasswordManager passwordManager,
            IPasswordRepository passwordRepository,
            IMapper mapper,
            IHaveIBeenPwnedRestClient pwnedClient) : base(logger, mapper)
        {
            _configuration = configuration;
            _repository = repository;
            _passwordManager = passwordManager;
            _passwordRepository = passwordRepository;
            _pwnedClient = pwnedClient;
        }

        [HttpGet("{userId}", Name = "UserGet")]
        public async Task<IActionResult> Get(int userId)
        {
            var user = await _repository.Get(userId);

            if (user == null) return NotFound();

            AddETag(user.RowVersion);

            return Ok(_mapper.Map<UserModel>(user));
        }

        // POST: api/aquarium
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserModel model)
        {
            try
            {
                var user = _mapper.Map<User>(model);

                if (user == null)
                {
                    return UnprocessableEntity();
                }

                if (user.Password == null)
                {
                    return BadRequest("Password cannot be null");
                }

                if (await _pwnedClient.IsPasswordPwned(user.Password))
                {
                    return BadRequest("Pwned Password");
                }

                var existingUser = await _repository.Get(user.Username);

                if (existingUser != null)
                {
                    return BadRequest("User already exists");
                }

                _logger.LogInformation("Creating new user...");
                await _repository.Add(user);
                _logger.LogInformation($"New user created. UserID:{user.Id}.");

                await AddPassword(user);

                AddETag(user.RowVersion);

                var url = Url.Link("UserGet", new { userId = user.Id });
                return Created(url, _mapper.Map<UserModel>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst trying to create User.");
            }
            return BadRequest("Could not create User");
        }

        private async Task AddPassword(User user)
        {
            var userPassword = new UserPassword(user.Id, _passwordManager.CreatePasswordHash(user.Password));

            await _passwordRepository.Add(userPassword);
        }

        private string HashPassword(string password)
        {
            return _passwordManager.CreatePasswordHash(password);
        }

        // PUT: api/aquarium/5
        [HttpPut("{userId}")]
        public async Task<IActionResult> Put(int userId, [FromBody]UserModel model)
        {
            try
            {
                var user = await _repository.Get(userId);

                if (user == null) return NotFound();

                _mapper.Map(model, user);

                _logger.LogInformation($"Updating user. UserID:{userId}");
                await _repository.Update(user);

                AddETag(user.RowVersion);

                return Ok(_mapper.Map<UserModel>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst trying to update User.");
            }
            return BadRequest("Could not update User");
        }

        // DELETE: api/aquarium/5
        [Authorize(Policy = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(int userId)
        {
            try
            {
                var user = await _repository.Get(userId);
                if (user == null) return NotFound();

                _logger.LogInformation($"Deleting user. UserID:{userId}, Username: {User.Identity.Name}");
                await _repository.Delete(user.Id);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst trying to delete User.");
            }
            return BadRequest("Could not delete User");
        }

        [HttpPost("{userId}/changepassword")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody]PasswordModel model)
        {
            try
            {
                if (model.NewPassword == model.OldPassword) return BadRequest("Passwords are the same");

                var user = await _repository.Get(userId);
                if (user == null) return NotFound();

                var oldPassword = _passwordManager.CreatePasswordHash(model.OldPassword);
                var newPassword = _passwordManager.CreatePasswordHash(model.OldPassword);

                if (user.Password != oldPassword)
                {
                    _logger.LogInformation($"Attempting to update user password. UserID:{userId}. Password Don't match.");
                    return BadRequest("Current passwords don't match");
                }

                if (user.Password == newPassword)
                {
                    _logger.LogInformation($"Attempting to update user password. UserID:{userId}. New password is the same as the current.");
                    return BadRequest("New password is the same as the current");
                }

                _logger.LogInformation($"Updating user password. UserID:{userId}");

                user.Password = newPassword;
                await AddPassword(user);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst trying to change Password.");
            }
            return BadRequest("Could not change Password");
        }
    }
}