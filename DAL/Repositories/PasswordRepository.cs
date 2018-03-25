using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL
{
    public class PasswordRepository : BaseRepository, IPasswordRepository
    {
        private const string getQuery = @"SELECT Created, PasswordHashAndSalt, LEAD(Created) OVER (ORDER BY Created) AS Expired
                              FROM dbo.UserPasswords
                              WHERE UserId = @userId
                              ORDER BY Created DESC";

        private const string insertQuery = @"INSERT INTO dbo.UserPasswords (UserId,PasswordHashAndSalt,Created)
                                            VALUES (@userId, @PasswordHashAndSalt, GETDATE())";

        public PasswordRepository(IConnectionFactory connectionFactory,
            ILogger<UserRepository> logger) : base(connectionFactory, logger)
        {

        }

        public async Task Add(UserPassword userPassword)
        {
            _logger.LogInformation($"Adding password. UserId:{userPassword.UserId}'...");
            try
            {
                using (var connection = _connectionFactory.GetOpenConnection())
                {
                    await connection.QueryAsync(insertQuery, new { userPassword.UserId, userPassword.PasswordHashAndSalt });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst adding a new password");
            }

            _logger.LogInformation($"Finished adding password. UserId:{userPassword.UserId}'.");
        }

        public async Task<List<UserPassword>> Get(int userId)
        {
            _logger.LogInformation($"Getting passwords for user. UserId:{userId}'...");
            List<UserPassword> passwords = null;

            try
            {
                using (var connection = _connectionFactory.GetOpenConnection())
                {
                    passwords = (await connection.QueryAsync<UserPassword>(getQuery, new { userId })).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured whilst getting a users passwords.");
            }

            _logger.LogInformation($"Finished getting passwords for user. UserId:'{userId}'.");
            return passwords;
        }
    }
}
