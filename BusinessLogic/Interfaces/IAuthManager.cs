using AquariumMonitor.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AquariumMonitor.BusinessLogic
{
    public interface IAuthManager
    {
        Claim[] CreateUserClaims(User user);

        JwtSecurityToken CreateToken(Claim[] claims, int validForDurationMinutes, SigningCredentials creds);

        Task<Claim[]> GetAllClaims(User user);

        SigningCredentials GetSigningCredentials();
    }
}