using AquariumMonitor.Models;
using System.Threading.Tasks;

namespace AquariumMonitor.BusinessLogic.Interfaces
{
    public interface IPasswordManager
    {
        Task<bool> VerifyPassword(int userId, string password);

        string CreatePasswordHash(string password);
    }
}
