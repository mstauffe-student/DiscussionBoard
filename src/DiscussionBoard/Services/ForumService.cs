using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;


namespace DiscussionBoard.Services
{
    /**
     * Class to provide serives to forum page.
     */
    public class ForumService : IForumService
    {
        private readonly ApplicationDbContext _context;

        /**
         * constructor to allow ApplicationDbContext to be injected.
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
