using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiscussionBoard.Services
{
    /**
     * Interface for ForumService for dependency injection.
     */
    public interface IForumService
    {
      Task<List<Forum>> GetAllAsync();

    }
}
