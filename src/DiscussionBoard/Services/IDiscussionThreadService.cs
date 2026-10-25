using DiscussionBoard.Models;

namespace DiscussionBoard.Services
{
    /**
     * Interface for DiscussionThreadService for dependency injection.
     */
    public interface IDiscussionThreadService
    {
        Task<DiscussionThread> GetAsync(int id);
        Task<List<DiscussionThread>> GetForForumAsync(int forumId);
        Task<DiscussionThread> AddAsync(DiscussionThread thread);
        Task UpdateAsync(DiscussionThread thread);
        Task<List<DiscussionThread>> GetAllAsync();

    }
}
