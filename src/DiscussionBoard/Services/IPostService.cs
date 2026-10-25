using DiscussionBoard.Models;

namespace DiscussionBoard.Services
{
    /**
     * Interface for PostService for dependency injection.
     */
    public interface IPostService
    {
        Task<List<Post>> GetForThreadAsync(int threadId);
        Task<Post> AddAsync(Post post);
    }
}
