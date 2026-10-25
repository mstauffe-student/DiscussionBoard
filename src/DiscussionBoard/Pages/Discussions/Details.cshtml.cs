using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    public class Details : PageModel
    {
        private readonly ILogger<Details> _logger;
        private readonly IPostService _post;
        [BindProperty]
        public UserInputModel Input { get; set; } = new();
        public List<PostTreeBuilder.Node> ReplyItems { get; private set; } = new();

        /// <summary>
        /// The Discussion/Details page, displaying all active posts for a discussion
        /// </summary>
        /// <param name="post">Service to manage posts</param>
        /// <param name="logger">Logging provider</param>
        public Details(IPostService post, ILogger<Details> logger)
        {
            _post = post;
            _logger  = logger;
        }

        /// <summary>
        /// Returns a list of the active posts of selected thread on the site
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                ReplyItems = PostTreeBuilder.Build(await _post.GetForThreadAsync(id));
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Sorry, but something went wrong." + e.Message;
                _logger.LogError($"ReplyItems failure in Discussions/Details. {e.Message}");
                throw;
            }
            if (ReplyItems == null  || ReplyItems.Count == 0)
            {
                TempData["IfEmpty"] = "No posts have been made in this thread yet.";
            }
            return Page();
        }

        /**
         * Allows a user to add a post to a thread. Users can only add content.
         * <param name="id">id of the thread</param>
         * <param name="content">post content to be added</param>
         */
        public async Task<IActionResult> OnPostCreate(int id, string content)
        {
            if (content == null || content == string.Empty)
            {
                //Makes sure a post is actually saying something.
                TempData["ErrorMessage"] = "Post can't be empty or less than 2 characters.";
                _logger.LogWarning($"Post {content} can't be empty or less than 2 characters.");
                return RedirectToPage();
            }

            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                //Stops an authorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized.";
                _logger.LogWarning($"User {user} is unauthorized and tried to create a post.");
                return RedirectToPage();
            }
            else
            {
                Input.Content = content;
                Post post = new Post()
                {
                    Title = "",
                    Content = Input.Content,
                    ApplicationUserId = user,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false,
                    DiscussionThreadId = id
                };
                try
                {
                    await _post.AddAsync(post);
                }
                catch (Exception e)
                {
                    //Hopefully catches if there's a database issue
                    Console.WriteLine(e.InnerException?.Message);
                    _logger.LogError($"Could not add the post {post}. {e.Message}");
                    TempData["ErrorMessage"] = "Something went wrong. Couldn't add this. " + e.Message;
                }
            }
            return RedirectToPage(new
            {
                Id = id,
            });
        }

        /**
         * Allows a user to add a reply to a post. Users can only add content.
         * <param name="threadId">id of the thread</param>
         */
        public async Task<IActionResult> OnPostReply(int threadId)
        {
            if (Input.Content == null || Input.Content == string.Empty)
            {
                //Makes sure a reply is actually saying something.
                TempData["ErrorMessage"] = "Reply can't be empty or less than 2 characters.";
                _logger.LogWarning($"Reply {Input.Content} can't be empty or less than 2 characters.");
                return RedirectToPage();
            }

            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                //Stops an authorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized.";
                _logger.LogWarning($"User {user} is unauthorized and tried to create a reply.");
                return RedirectToPage();
            }
            else
            {
                Post reply = new Post()
                {
                    Title = "",
                    Content = Input.Content,
                    ApplicationUserId = user,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false,
                    DiscussionThreadId = threadId,
                    ParentPostId = Input.ParentPostId
                };
                try
                {
                    await _post.AddAsync(reply);
                }
                catch (Exception e)
                {
                    //Hopefully catches if there's a database issue
                    Console.WriteLine(e.InnerException?.Message);
                    TempData["ErrorMessage"] = "Something went wrong. Couldn't add this. " + e.Message;
                    _logger.LogError($"Could not add the reply {reply}. {e.Message}");
                }
            }
            return RedirectToPage(new
            {
                threadID = threadId,
            });
        }
    }
}
