using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SubRedditStatus.Models;
using SubRedditStatus.Shared.Models;
using System.Net;
using System.Net.Http;
using Xunit;

namespace SubRedditStatus.API.Tests
{
    public class RedditApiServiceTest: IClassFixture<WebApplicationFactory<Program>>
    {
        readonly HttpClient _client;

        public RedditApiServiceTest(WebApplicationFactory<Program> application)
        {
            _client = application.CreateClient();
        }

        [Fact]
        public async Task ReturnsTop10Posts()
        {
            string jwt = await ApiTokenHelper.GetJWT("ConsoleApp", "Test1234$", _client);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
            var response = await _client.GetAsync("/api/subreddit/science/10");
            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<SubredditTopResponse>(stringResponse);

            Assert.Equal(10, model!.TopPosts.Count());
        }

        [Fact]
        public async Task GET_Throw_Responds_404()
        {
            using var response = await _client.GetAsync("/api/subreddit");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GET_Throw_Responds_401()
        {
            using var response = await _client.GetAsync("/api/subreddit/science/test");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}