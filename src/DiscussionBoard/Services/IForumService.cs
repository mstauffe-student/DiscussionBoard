using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
