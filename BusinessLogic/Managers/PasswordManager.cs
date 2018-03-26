using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using Serilog;
using Sodium;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquariumMonitor.BusinessLogic
{
    public class PasswordManager : IPasswordManager
    {
        private readonly IPasswordRepository _passwordRepository;
        private readonly ILogger _logger;

        public PasswordManager(IPasswordRepository passwordRepository, ILogger logger)
        {
            _passwordRepository = passwordRepository;
            _logger = logger;
        }

        public async Task<bool> VerifyPassword(int userId, string password)
        {
            var passwordHashes = await _passwordRepository.Get(userId);

            if(passwordHashes == null || passwordHashes.Count == 0)
            {
                _logger.Information($"User:{userId} has no passwords.");
                return false;
            }

            // Check current password
            if (CheckPassword(passwordHashes[0].PasswordHashAndSalt, password))
            {
                return true;
            }

            // Check historic passwords just and log if match
            CheckExpiredPasswords(userId, passwordHashes, password);
            return false;
        }

        private void CheckExpiredPasswords(int userId, List<UserPassword> passwordHashes, string password)
        {
            _logger.Information($"Started checking historic passwords for User:{userId}");

            foreach (var oldPasswordHash in passwordHashes)
            {
                if(CheckPassword(oldPasswordHash.PasswordHashAndSalt, password))
                {
                    _logger.Information($"User:{userId} suppplied old password. Expired:{oldPasswordHash.Expired}");
                }
            }

            _logger.Information($"Finished checking historic passwords for User:{userId}");
        }

        private bool CheckPassword(string passwordHashAndSalt, string password)
        {
            return PasswordHash.ScryptHashStringVerify(passwordHashAndSalt, password);
        }

        public string CreatePasswordHash(string password)
        {
            return PasswordHash.ScryptHashString(password, PasswordHash.Strength.MediumSlow);
        }
    }
}
