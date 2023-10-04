using SubRedditStatus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRedditStatus.Shared.Models
{
    public class SubredditTopResponse
    {
        public int RateLimitUsed { get; set; }
        public int RateLimitRemaining { get; set; }
        public int RateLimitReset { get; set; }
        public IEnumerable<TopPost> TopPosts { get; set; }
    }
}
