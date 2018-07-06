using Dapper;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AquariumMonitor.DAL
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private const string GetByUserNameQuery = @"SELECT Id,Username,FirstName,LastName,Email,[RowVersion], 
                                                (SELECT TOP 1 PasswordHashAndSalt FROM dbo.UserPasswords ORDER BY Created DESC) AS Password
                                                FROM Users WHERE Username = @userName";

        private const string GetByIdQuery = @"SELECT Id,Username,FirstName,LastName,Email,[RowVersion], 
                                                (SELECT TOP 1 PasswordHashAndSalt FROM dbo.UserPasswords ORDER BY Created DESC) AS Password
                                                FROM Users WHERE Id = @id";

        private const string InsertQuery = @"INSERT INTO Users (Username,FirstName,LastName,Email)
                                             VALUES (@Username,@FirstName,@LastName,@Email);
                                             SELECT CAST(SCOPE_IDENTITY() AS INT);";

        private const string UpdateQuery = @"UPDATE Users SET FirstName = @FirstName,LastName = @LastName, Email = @Email
                                             WHERE Id = @Id;";

        private const string DeleteQuery = @"UPDATE Users SET Deleted = 1 WHERE Id = @Id;";

        private const string UpdateLastLoginQuery = @"UPDATE Users SET LastLogin = @LastLogin WHERE Id = @Id;";

        private const string UserClaimsQuery = @"SELECT Name FROM vw_UserClaims WHERE UserId = @userId;";
        
        private string ExistsQuery = "SELECT 'Exists' FROM Users WHERE Id = @id";

        public UserRepository(IConnectionFactory connectionFactory,
            ILogger<UserRepository> logger) : base (connectionFactory, logger)
        { 
        }

        public async Task Add(User user)
        {
            _logger.LogInformation($"Adding User. Username:{user.Username}'...");

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                user.Id = await connection.QueryFirstAsync<int>(InsertQuery, user);
            }

             _logger.LogInformation($"Finished adding User. UserId:'{user.Id}'.");
        }

        public async Task Delete(int Id)
        {
            _logger.LogInformation($"Deleting User. UserId:'{Id}'...");

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                await connection.ExecuteAsync(DeleteQuery, new { Id });
            }

            _logger.LogInformation($"Finished deleting User. UserId:'{Id}'.");
        }

        public async Task<bool> Exists(int userId)
        {
            _logger.LogInformation($"Checking User Exists by Id:'{userId}'...");
            string exists;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                exists = await connection.QueryFirstOrDefaultAsync<string>(ExistsQuery, new { userId });
            }

            _logger.LogInformation($"Finished checking User Exists. UserId:'{userId}', Exists:{exists}.");

            return exists != null ? true : false;
        }

        public async Task<User> Get(int Id)
        {
            _logger.LogInformation($"Getting User by Id:'{Id}'...");
            User user;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                user = await connection.QueryFirstOrDefaultAsync<User>(GetByIdQuery, new { Id });
            }
            _logger.LogInformation($"Finished getting User by Id:'{Id}'.");
            return user;
        }

        public async Task<User> Get(string userName)
        {
            _logger.LogInformation($"Getting User by username:'{userName}'...");
            User user;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                user = await connection.QueryFirstOrDefaultAsync<User>(GetByUserNameQuery, new { userName });
            }
            _logger.LogInformation($"Finished getting User by username:'{userName}'.");
            return user;
        }

        public async Task<List<Claim>> GetClaims(int UserId)
        {
            _logger.LogInformation($"Getting User claims by UserId:'{UserId}'...");
            List<Claim> claims;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                var results = await connection.QueryAsync<string>(UserClaimsQuery, new { UserId });

                claims = new List<string>(results).Select(c => new Claim(c.ToString(), "True")).ToList();
            }
            _logger.LogInformation($"Finished getting User claims by UserId:'{UserId}'.");

            return claims;
        }

        public async Task Update(User user)
        {
            _logger.LogInformation($"Updating User. UserId:'{user.Id}'...");

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                await connection.ExecuteAsync(UpdateQuery, user);
            }

            _logger.LogInformation($"Finshed updating User. UserId:'{user.Id}'.");
        }

        public async Task UpdateLastLogin(int id)
        {
            _logger.LogInformation($"Updating User Last Login. UserId:'{id}'...");

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                await connection.ExecuteAsync(UpdateLastLoginQuery, new { id, lastLogin = DateTimeOffset.Now });
            }

            _logger.LogInformation($"Finished updating User Last Login. UserId:'{id}'.");
        }
    }
}
