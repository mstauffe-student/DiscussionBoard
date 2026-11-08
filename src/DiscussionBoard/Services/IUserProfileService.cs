using DiscussionBoard.Models;

namespace DiscussionBoard.Services
{
    public interface IUserProfileService
    {
        Task<List<ApplicationUser>> GetAllAsync();
        Task<bool> IsDisplayNameAvailableAsync(string displayName, string?
            excludeUserId = null);

    }
}
