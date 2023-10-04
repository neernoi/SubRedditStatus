using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using SubRedditStatus.Interfaces;
using SubRedditStatus.Models;
using SubRedditStatus.Shared.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SubRedditStatus.Services
{
    public class SubRedditTopPostsService : ISubRedditTopPostsService
    {
        private readonly IConfiguration _config;

        // Policy for Rate Limit and Rate Reset
        IAsyncPolicy<HttpResponseMessage> retryPolicy =
    Policy.Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.StatusCode == (HttpStatusCode)429) // RetryAfter
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: (retryCount, response, context) => {
                var responseResult = response.Result;
                HttpHeaders headers = responseResult.Headers;
                IEnumerable<string> values;
                if (headers.TryGetValues("X-Ratelimit-Reset", out values))
                {
                    int rateLimitReset = values.Count() > 0 ? Convert.ToInt32(values.First().Split('.')[0]) : 0;
                    return TimeSpan.FromSeconds(rateLimitReset);
                }
                return TimeSpan.FromSeconds(1);
            },
            (_, __, ___, ____) => Task.CompletedTask);
        
        public SubRedditTopPostsService(IConfiguration config)
        {
            _config = config;
        }
        public async Task<SubredditTopResponse> GetSubredditTopPosts(string subreddit, string accessToken, int limit)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // Set the user agent and access token in the request headers
                httpClient.DefaultRequestHeaders.Add("User-Agent", _config["Reddit:UserAgent"]); // Replace with your user agent
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                // Json top response from reddit api
                var response = await retryPolicy.ExecuteAsync(() => httpClient.GetAsync($"{_config["Reddit:SubRedditUrl"]}{subreddit}/top/.json?limit={limit}"));

                // Get Response Header Values
                int rateLimitUsed = 0;
                int rateLimitRemaining = 0;
                int rateLimitReset = 0;
                HttpHeaders headers = response.Headers;
                IEnumerable<string> values;
                if (headers.TryGetValues("X-Ratelimit-Used", out values))
                {
                    rateLimitUsed = values.Count() > 0 ? Convert.ToInt32(values.First()) : 0;
                }
                if (headers.TryGetValues("X-Ratelimit-Remaining", out values))
                {
                    rateLimitRemaining = values.Count() > 0 ? Convert.ToInt32(values.First().Split('.')[0]) : 0;
                }
                if (headers.TryGetValues("X-Ratelimit-Reset", out values))
                {
                    rateLimitReset = values.Count() > 0 ? Convert.ToInt32(values.First().Split('.')[0]) : 0;
                }

                // If response is success
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseContent);
                    var data = json["data"]["children"].Select(x => new TopPost
                    {
                        Title = x["data"]["title"].ToString(),
                        Score = Convert.ToInt32(x["data"]["score"]),
                        UpVotes = Convert.ToInt32(x["data"]["ups"]),
                        Author = x["data"]["author"].ToString()
                    });
                    return new SubredditTopResponse
                    {
                        RateLimitRemaining = rateLimitRemaining,
                        RateLimitReset = rateLimitReset,
                        RateLimitUsed = rateLimitUsed,
                        TopPosts = data
                    };
                }

                return new SubredditTopResponse(); // Error
            }
        }
    }
}
