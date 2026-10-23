using System.Data.Common;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services

{
    public class DiscussionThreadService : IDiscussionThreadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscussionThreadService> _logger;

        /**
         * constructor to allow ApplicationDbContext to be injected.
         */
        public DiscussionThreadService(ApplicationDbContext context, ILogger<DiscussionThreadService> logger)
        {
            _context = context;
            _logger = logger;
        }


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

        public async Task UpdateAsync(int id, DiscussionThread thread)
        {
            var entity = await _context.DiscussionThreads.FindAsync(id);
            if (entity == null)
            {
                _logger.LogError("Couldn't find that thread");
                throw new KeyNotFoundException();
            }
            entity.Title =  thread.Title;
            entity.Content =  thread.Content;
            entity.Posts = thread.Posts;
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
