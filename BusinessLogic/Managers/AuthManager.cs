using AquariumMonitor.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using AquariumMonitor.DAL.Interfaces;
using System.Linq;
using System.Text;

namespace AquariumMonitor.BusinessLogic
{
    public class AuthManager : IAuthManager
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public AuthManager(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<Claim[]> GetAllClaims(User user)
        {
            var userClaims = await _userRepository.GetClaims(user.Id);

            var claims = CreateUserClaims(user);

            claims.Union(userClaims);

            return claims;
        }

        public SigningCredentials GetSigningCredentials()
        {
            var tokenSecurityKey = Environment.GetEnvironmentVariable(Constants.TokenSecurityKey);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return signingCredentials;
        }


        public Claim[] CreateUserClaims(User user)
        {
            return new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };
        }


        public JwtSecurityToken CreateToken(Claim[] claims, int validForDurationMinutes, SigningCredentials creds)
        {
            return new JwtSecurityToken
                (
                    issuer: _configuration[Constants.ValidIssuer],
                    audience: _configuration[Constants.ValidAudience],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(validForDurationMinutes),
                    signingCredentials: creds
                );
        }
    }
}
