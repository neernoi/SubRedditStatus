using SubRedditStatus.Models;
using SubRedditStatus.Shared.Models;

namespace SubRedditStatus.Interfaces
{
    public interface ISubRedditTopPostsService
    {
        Task<SubredditTopResponse> GetSubredditTopPosts(string subreddit, string accessToken, int limit);
    }
}
