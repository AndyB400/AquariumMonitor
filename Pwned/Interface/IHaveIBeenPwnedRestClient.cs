using System.Threading.Tasks;

namespace Pwned
{
    public interface IHaveIBeenPwnedRestClient
    {
        Task<bool> IsPasswordPwned(string password);
    }
}
