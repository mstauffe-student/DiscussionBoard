using DiscussionBoard.Models;

namespace DiscussionBoard.Services
{
    public interface IDiscussionThreadService
    {
        Task<DiscussionThread> GetAsync(int id);
        Task<List<DiscussionThread>> GetForForumAsync(int forumId);
        Task<DiscussionThread> AddAsync(DiscussionThread thread);
        Task UpdateAsync(int id, DiscussionThread thread);


    }
}
