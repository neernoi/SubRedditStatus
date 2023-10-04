namespace SubRedditStatus.Interfaces
{
    public interface IRedditAccessService
    {
        Task<string?> GetAccessToken(string? clientId, string? clientSecret);
    }
}
