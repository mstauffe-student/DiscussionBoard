using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services
{
    public class UserProfileService : IUserProfileService
    {

        private readonly ApplicationDbContext _context;
        // private readonly ILogger<PostService> _logger;

        /**
         * constructor to allow ApplicationDbContext and ILogger to be injected.
         * <param name="context">DbContext to inject</param>
         * <param name="logger">logger to inject</param>
         */
        public UserProfileService(ApplicationDbContext context)
        {
            _context = context;
            // _logger = logger;
        }

        /**
         * Gets all users and puts it in a list.
         */
        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            try
            {
                return await _context.Users
                    .OrderBy(au => au.UserName )
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> IsDisplayNameAvailableAsync(string displayName, string?
            excludeUserId = null)
        {
            displayName.Trim();
            if (displayName.Length < 2 || displayName.Length > 60)
            {
                return false;
            }

            return !await _context.Users.AnyAsync(au => au.DisplayName.ToLower().Trim().Equals(displayName.ToLower().Trim())
            && au.Id != excludeUserId);
        }

    }
}
