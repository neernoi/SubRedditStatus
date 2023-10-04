using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubRedditStatus.Interfaces;
using SubRedditStatus.Models;

namespace SubRedditStatus.Controllers
{
    [ApiController]
    [Route("api/subreddit")]
    public class SubRedditDataController : ControllerBase
    {
        private readonly IRedditAccessService _redditAccess;
        private readonly ISubRedditTopPostsService _subRedditTopPosts;
        private readonly ILogger<SubRedditDataController> _logger;
        private readonly IConfiguration _config;

        public SubRedditDataController(IConfiguration config, IRedditAccessService redditAccess, ISubRedditTopPostsService subRedditTopPosts, ILogger<SubRedditDataController> logger)
        {
            _config = config;
            _logger = logger;
            _redditAccess = redditAccess;
            _subRedditTopPosts = subRedditTopPosts;
        }

        /// <summary>
        /// Returns top posts for a subreddit
        /// </summary>
        /// <param name="subreddit">Name of the subreddit</param>
        /// <param name="limit"> Data limit</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("{subreddit}/{limit}", Name = "GetTopPosts")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<IEnumerable<TopPost>>> GetTopPosts(string subreddit, int limit)
        {
            string clientId = _config["Reddit:ClientId"];
            string clientSecret = _config["Reddit:ClientSecret"];
            string accessToken = await _redditAccess.GetAccessToken(clientId, clientSecret);
            if (!string.IsNullOrEmpty(accessToken))
            {
                var topPosts = await _subRedditTopPosts.GetSubredditTopPosts(subreddit, accessToken, limit);
                return Ok(topPosts);
            }
            else
            {
                _logger.Log(LogLevel.Error, "Failed to obtain an access token.", DateTime.Now);
                throw new Exception("Failed to obtain an access token.");
            }

        }
    }
}