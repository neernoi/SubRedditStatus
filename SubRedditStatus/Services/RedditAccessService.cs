using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using SubRedditStatus.Helpers;
using SubRedditStatus.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http;

namespace SubRedditStatus.Services
{
    public class RedditAccessService : IRedditAccessService
    {
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        public RedditAccessService(IConfiguration config, IMemoryCache memoryCache)
        {
            _config = config;
            _memoryCache = memoryCache;
        }
        public async Task<string?> GetAccessToken(string? clientId, string? clientSecret)
        {
            // Check if the token is cached
            string accessToken;
            accessToken = new CacheHelper(_memoryCache).GetMemory(clientId) as string;

            if (!string.IsNullOrEmpty(accessToken))
                return accessToken;

            using (HttpClient httpClient = new HttpClient())
            {
                // Set the user agent
                httpClient.DefaultRequestHeaders.Add("User-Agent", _config["Reddit:UserAgent"]);

                // Prepare the request data
                var requestData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
            };

                // Set the authorization header
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"))
                );

                // Make a POST request to obtain the access token
                HttpResponseMessage response = await httpClient.PostAsync(_config["Reddit:AccessUrl"], new FormUrlEncodedContent(requestData));

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseContent);
                    accessToken = json.GetValue("access_token").ToString();
                    new CacheHelper(_memoryCache).SetMemory(clientId, accessToken, 86350);
                    return accessToken;
                }

                return null;
            }
        }
    }
}
