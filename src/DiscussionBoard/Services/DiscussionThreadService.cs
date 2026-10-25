using System.Data.Common;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services

{
    /**
     * Class to allow database services/features for Discussion threads.
     */
    public class DiscussionThreadService : IDiscussionThreadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscussionThreadService> _logger;

        /**
         * constructor to allow ApplicationDbContext and ILogger to be injected.
         * <param name="context">DbContext to inject</param>
         * <param name="logger">logger to inject</param>
         */
        public DiscussionThreadService(ApplicationDbContext context, ILogger<DiscussionThreadService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /**
         * Gets all not deleted threads and puts it in a list.
         */
        public async Task<List<DiscussionThread>> GetAllAsync()
        {
            try
            {
                return await _context.DiscussionThreads
                    .Where(t => t.IsDeleted == false)
                    .OrderBy(t => t.Title )
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /**
         * Gets 1 thread based on its id.
         * <param name="id">id of a thread</param>
         */
        public async Task<DiscussionThread> GetAsync(int id)
        {
            try
            {
                return await _context.DiscussionThreads
                    .FindAsync(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError($"Couldn't get DiscussionThread with id {id}, {e.Message}");
                throw;
            }
        }

        /**
         * Gets a list of threads based on its forum id.
         * <param name="forumId">id of a thread's forumId</param>
         */
        public async Task<List<DiscussionThread>> GetForForumAsync(int forumId)
        {
            try
            {
                return await _context.DiscussionThreads
                    .Where(dt => dt.ForumId == forumId)
                    .OrderBy(dt => dt.Title)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError($"Couldn't get DiscussionThread Forum with id {forumId}, {e.Message}");
                throw;
            }
        }

        /**
         * Adds a thread. When used for Page Models, only Content and Title should be allowed to be named by
         * a user. The rest should be generated behind the scenes.
         * <param name="thread">the new thread to be added to the database</param>
         */
        public async Task<DiscussionThread> AddAsync(DiscussionThread thread)
        {
            try
            {
                 _context.DiscussionThreads.Add(thread);
                await _context.SaveChangesAsync();
                return thread;
            }
            catch (DbException e)
            {
                Console.WriteLine(e);
                _logger.LogError($"Couldn't add DiscussionThread {thread}, {e.Message}");
                throw;
            }
        }

        /**
         * Updates a thread. Only Content and Title should be allowed to be edited by
         * a user. The rest should be generated behind the scenes.
         * <param name="thread">the thread to be changed</param>
         */
        public async Task UpdateAsync(DiscussionThread thread)
        {
            var update = await _context.DiscussionThreads.FindAsync(thread.Id);
            if (update == null)
            {
                _logger.LogError("Couldn't find that thread");
                throw new KeyNotFoundException();
            }
            update.Title =  thread.Title;
            update.Content =  thread.Content;
            update.Posts = thread.Posts;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError($"Couldn't update DiscussionThread {thread}, {e.Message}");
                throw;
            }
        }
    }
}
