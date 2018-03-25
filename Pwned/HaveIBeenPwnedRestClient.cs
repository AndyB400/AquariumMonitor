using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pwned
{
    public class HaveIBeenPwnedRestClient : IHaveIBeenPwnedRestClient
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly string URL = @"https://api.pwnedpasswords.com";

        public HaveIBeenPwnedRestClient()
        {

        }

        public async Task<bool> IsPasswordPwned(string password)
        {
            var apiMethod = "range";
            var passwordHash = CreateSHA1Hash(password); 

            var response = await GetRequestAsync($"{apiMethod}/{passwordHash.Substring(0, 5)}");

            if (response.StatusCode == "OK")
            {
                return CheckResults(passwordHash, response);
            }
            else
            {
                return false;
            }
        }

        private bool CheckResults(string passwordHash, Response response)
        {
            var hashes = response.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(h => $"{passwordHash.Substring(0, 5)}{h.Split(':')[0]}").ToList();

            if(hashes.Contains(passwordHash))
                return true;

            return false;
        }

        private string CreateSHA1Hash(string password)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] data = sha.ComputeHash(Encoding.UTF8.GetBytes(password));

            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString().ToUpper();
        }

        private async Task<Response> GetRequestAsync(string parameters)
        {
            Response RestResponse = new Response();
            Uri uri = new Uri($"{URL}/{parameters}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            HttpResponseMessage response = null;
            request.Headers.TryAddWithoutValidation("User-Agent", "AquaMonitor");
            request.Headers.TryAddWithoutValidation("api-version", "2");

            try
            {
                response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                string statusCode = response.StatusCode.ToString();

                RestResponse.Body = responseBody;
                RestResponse.StatusCode = statusCode;

                return RestResponse;
            }
            catch (HttpRequestException e)
            {
                RestResponse.Body = null;
                RestResponse.StatusCode = response.StatusCode.ToString();
                RestResponse.HttpException = e.Message;
                return RestResponse;
            }
        }
    }
}