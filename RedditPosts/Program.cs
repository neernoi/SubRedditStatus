using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SubRedditStatus.Models;
using SubRedditStatus.Shared.Models;

class Program
{
    static async Task Main(string[] args)
    {
        // Make a request to the Reddit API to get the top posts from a subreddit
        string subreddit = "funny"; // Change to the desired subreddit

        int requestCounter = 1;
        int requestsRemaining = 599;
        try
        {
            Thread.Sleep(5000); // Make sure webapi project starts and running
            await GetRealTimeSubRedditStatus(subreddit, requestCounter, requestsRemaining);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadLine();
            throw;
        }


    }

    private static async Task GetRealTimeSubRedditStatus(string subreddit, int requestCounter, int requestsRemaining)
    {
        while (requestsRemaining > 0)
        {
            var topResponse = await GetTopPosts(subreddit, 100);
            var topPosts = topResponse.TopPosts;
            requestCounter = topResponse.RateLimitUsed;
            requestsRemaining = topResponse.RateLimitRemaining;
            int resetDuration = topResponse.RateLimitReset;

            if (topPosts.Count() > 0)
            {
                // Sort the dictionary by post count in descending order
                var topUsers = await GetTopUsers(topPosts);

                var sortedUsers = topUsers.OrderByDescending(kv => kv.Value);

                Console.Clear();

                // Display the top posts
                Console.WriteLine($"==== TOP 3 POSTS WITH MOST UPVOTES ==== Request Number {requestCounter} == Remaining Requests {requestsRemaining} == Reset time in {resetDuration} seconds");
                Console.WriteLine();
                foreach (var post in topPosts.Take(3))
                {
                    Console.WriteLine($"Title: {post.Title}");
                    Console.WriteLine($"Upvotes: {post.UpVotes}");
                    Console.WriteLine($"Author: {post.Author}");
                    Console.WriteLine();
                }

                // Display the top users
                Console.WriteLine($"==== TOP 3 USERS WITH MOST POSTS ====");
                Console.WriteLine();
                foreach (var post in sortedUsers.Take(3))
                {
                    Console.WriteLine($"Author: {post.Key}");
                    Console.WriteLine($"Score: {post.Value}");
                    // Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Failed to get data from Reddit API.");
                return;
            }

            if (requestsRemaining == 0)
            {
                requestCounter = 1;
                requestsRemaining = 599;
                await Task.Delay((resetDuration + 2) * 1000);
            }
            else
                await Task.Delay(300);
        }
    }

    static async Task<SubredditTopResponse> GetTopPosts(string subreddit, int limit)
    {
        string jwt = await GetJWT("ConsoleApp", "Test1234$");
        if (!string.IsNullOrEmpty(jwt))
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
                // Make a GET request to fetch the top posts in the subreddit
                var response = await httpClient.GetStringAsync($"http://localhost:5249/api/subreddit/{subreddit}/{limit}");

                var posts = string.IsNullOrEmpty(response) ? new SubredditTopResponse() : JsonConvert.DeserializeObject<SubredditTopResponse>(response);
                return posts;
            }
        }
        return new SubredditTopResponse();
    }

    static async Task<Dictionary<string, int>> GetTopUsers(IEnumerable<TopPost> topPosts)
    {
        Dictionary<string, int> userPostCount = new Dictionary<string, int>();

        // Iterate through the top posts and count user posts
        await Task.Run(() =>
        {
            Parallel.ForEach(topPosts, post =>
            {
                lock (userPostCount)
                {
                    string author = post.Author;
                    if (!string.IsNullOrEmpty(author))
                    {
                        if (userPostCount.ContainsKey(author))
                        {
                            userPostCount[author]++;
                        }
                        else
                        {
                            userPostCount[author] = 1;
                        }
                    }
                }
            });

        });
        return userPostCount;
    }

    static async Task<string?> GetJWT(string username, string password)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(contentType);
            // Prepare the request data
            var requestData = new Dictionary<string, string>
            {
                {"username",$"{username}"},
                {"password",$"{password}"}
            };

            // Make a POST request to obtain the access token
            HttpResponseMessage response = await httpClient.PostAsync("http://localhost:5249/api/token", new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(responseContent);
            }

            return null;
        }
    }


}
