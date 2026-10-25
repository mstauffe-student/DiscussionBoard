using DiscussionBoard.Data;
using DiscussionBoard.Data.Migrations;
using System.Data.Common;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;


namespace DiscussionBoard.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostService> _logger;

        /**
         * constructor to allow ApplicationDbContext and ILogger to be injected.
         * <param name="context">DbContext to inject</param>
         * <param name="logger">logger to inject</param>
         */
        public PostService(ApplicationDbContext context, ILogger<PostService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /**
         * Gets a list of posts based on its thread id.
         * <param name="threadId">id of a post's threadId</param>
         */
        public async Task<List<Post>> GetForThreadAsync(int threadId)
        {
            try
            {
                return await _context.Posts
                    .Where(p => p.DiscussionThreadId == threadId)
                    .Where(p => p.IsDeleted == false)
                    .OrderBy(p => p.Title)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError($"Couldn't get Post with id {threadId}, {e.Message}");
                throw;
            }
        }

        /**
         * Adds a post. When used for Page Models, only Content should be allowed to be named by
         * a user. The rest should be generated behind the scenes.
         * <param name="post">the new post to be added to the database</param>
         */
        public async Task<Post> AddAsync(Post post)
        {

            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                return post;
            }
            catch (DbException e)
            {
                Console.WriteLine(e);
                _logger.LogError($"Couldn't add Post {post}, {e.Message}");
                throw;
            }
        }
    }
}
