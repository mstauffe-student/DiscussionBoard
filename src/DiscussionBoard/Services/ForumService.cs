using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;


namespace DiscussionBoard.Services
{
    /**
     * Class to allow database services/features for Forums.
     */
    public class ForumService : IForumService
    {
        private readonly ApplicationDbContext _context;

        /**
         * constructor to allow ApplicationDbContext and ILogger to be injected.
         * <param name="context">DbContext to inject</param>
         * <param name="logger">logger to inject</param>
         */
        public ForumService(ApplicationDbContext context)
        {
            _context = context;
        }

        /**
         * Gives a list of all Forums that aren't deleted.
         */
        public async Task<List<Forum>> GetAllAsync()
        {
            try
            {
                return await _context.Forums
                    .Where(f => f.IsDeleted == false)
                    .OrderBy(f => f.Name)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
