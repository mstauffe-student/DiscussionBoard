using DiscussionBoard.Models;

namespace DiscussionBoard.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetForThreadAsync(int threadId);
        Task<Post> AddAsync(Post post);
    }
}
