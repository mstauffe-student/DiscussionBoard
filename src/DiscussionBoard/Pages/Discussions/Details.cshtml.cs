using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    public class Details : PageModel
    {
        private readonly ILogger<Details> _logger;
        private readonly IPostService _post;
        [BindProperty]
        public UserInputModel Input { get; set; } = new();
        public List<PostTreeBuilder.Node> ReplyItems { get; private set; } = new();

        public List<Post> Authors = new();

        public DiscussionThread? ThreadAuthors;
        public ApplicationUser? CurrentUser;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// default constructor to inject services.
        /// </summary>
        /// <param name="post">Service to manage posts</param>
        /// <param name="logger">Logging provider</param>
        /// <param name="userManager">Service to manage users</param>
        public Details(IPostService post, ILogger<Details> logger, UserManager<ApplicationUser> userManager)
        {
            _post = post;
            _logger  = logger;
            _userManager = userManager;

        }


        /// <summary>
        /// Returns a list of the active posts of selected thread on the site
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                DiscussionThread? validThread = await _post.GetPostThreadAsync(id);
                if (validThread == null)
                {
                    _logger.LogWarning("Could not find that thread. That thread likely doesn't exist.");
                    return NotFound();
                }

                ThreadAuthors = await _post.GetPostThreadAsync(id); //Allows thread to access user data for name and avatar
                Authors = await _post.GetForAuthorsAsync(); //Allows posts to access user data for name and avatar
                ReplyItems = PostTreeBuilder.Build(await _post.GetForThreadAsync(id));
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Sorry, but something went wrong with loading the posts, replies, " +
                                           "and authors." + e.Message;
                _logger.LogError("Failure to load replies and authors Discussions/Details. {Message}", e.Message);
                throw;
            }
            if (ReplyItems == null  || ReplyItems.Count == 0)
            {
                TempData["IfEmpty"] = "No posts have been made in this thread yet.";
            }
            return Page();
        }

        /// <summary>
        /// Allows a user to add a post to a thread. Users can only add content.
        /// </summary>
        /// <param name="id">id of the thread</param>
        /// <param name="content">post content to be added</param>
        public async Task<IActionResult> OnPostCreate(int id, string content)
        {
            DiscussionThread? threadOfPost = null; //for logging
            Forum? forumOfPost = null; //for logging
            try
            {
                threadOfPost = await _post.GetPostThreadAsync(id); //For logging
                forumOfPost = await _post.GetPostForumAsync(threadOfPost.ForumId); //For logging
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Could not load thread or forum for logging.");
                throw;
            }

            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) //Sets up for logging
            {
                //Stops an unauthorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized!";
                _logger.LogWarning("An unauthorized user tried to create a post! Within thread '{thread}' in forum '{forum}'",
                    threadOfPost?.Title, forumOfPost?.Name);
                return RedirectToPage();
            }
            if (string.IsNullOrEmpty(CurrentUser.DisplayName))
            {
                CurrentUser.DisplayName = "Unconfigured User";
            }

            if (content == null || content == string.Empty)
            {
                //Makes sure a post is actually saying something.
                TempData["ErrorMessage"] = "Post can't be empty or less than 2 characters.";
                _logger.LogWarning("User '{User}' tried to create post '{content}' in thread '{thread}' in forum '{forum}' that's empty or less than 2 characters.",
                    CurrentUser.DisplayName, Input.Content, threadOfPost?.Title, forumOfPost?.Name);
                return RedirectToPage();
            }

            string? user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                //Stops an unauthorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized!";
                _logger.LogWarning("An unauthorized user tried to create a post! Within thread '{thread}' in forum '{forum}'",
                    threadOfPost?.Title, forumOfPost?.Name);
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
                    _logger.LogError("Could not add post! User '{User}' failed to add post '{Content}' in thread '{thread}' in forum '{forum}'. {ErrorMessage}"
                        ,CurrentUser.DisplayName, Input.Content, threadOfPost?.Title, forumOfPost?.Name, e.Message);
                    return RedirectToPage();
                }

                _logger.LogInformation("User '{User}' created post '{Content}' in thread '{thread}' in forum '{forum}'.",
                    CurrentUser.DisplayName, post.Content, threadOfPost?.Title, forumOfPost?.Name);
            }

            return RedirectToPage(new
            {
                Id = id,
            });
        }

        /// <summary>
        /// Allows a user to add a reply to a post. Users can only add content.
        /// </summary>
        ///<param name="threadId">id of the thread</param>
        public async Task<IActionResult> OnPostReply(int threadId)
        {
            DiscussionThread? threadOfReply = null; //for logging
            Forum? forumOfReply = null; //for logging
            try
            {
                threadOfReply = await _post.GetPostThreadAsync(threadId); //For logging
                forumOfReply = await _post.GetPostForumAsync(threadOfReply.ForumId); //For logging
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Could not load thread or forum for logging.");
                throw;
            }

            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) //Sets up for logging
            {
                //Stops an unauthorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized!";
                _logger.LogWarning("An unauthorized user tried to create a reply! Within thread '{thread}' in forum '{forum}'",
                    threadOfReply?.Title, forumOfReply?.Name);
                return RedirectToPage();
            }
            if (string.IsNullOrEmpty(CurrentUser.DisplayName))
            {
                CurrentUser.DisplayName = "Unconfigured User";
            }

            if (string.IsNullOrEmpty(Input.Content))
            {
                //Makes sure a reply is actually saying something.
                TempData["ErrorMessage"] = "Reply can't be empty or less than 2 characters.";
                _logger.LogWarning("User '{User}' tried to create reply '{content}' in thread '{thread}' in forum '{forum}' that's empty or less than 2 characters.",
                    CurrentUser.DisplayName, Input.Content, threadOfReply?.Title, forumOfReply?.Name);
                return RedirectToPage();
            }

            string? user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                //Stops an unauthorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized.";
                _logger.LogWarning("An unauthorized user tried to create a reply! Within thread '{thread}' in forum '{forum}'",
                    threadOfReply?.Title, forumOfReply?.Name);
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
                    TempData["ErrorMessage"] = "Something went wrong. Couldn't add this reply. " + e.Message;
                    _logger.LogError("Could not add reply! User '{User}' failed to add reply '{Content}' in thread '{thread}' in forum '{forum}'. {ErrorMessage}"
                        ,CurrentUser.DisplayName, Input.Content, threadOfReply?.Title, forumOfReply?.Name, e.Message);
                    return RedirectToPage();
                }
                _logger.LogInformation("User '{User}' created reply '{Content}' in thread '{thread}' in forum '{forum}'.",
                    CurrentUser.DisplayName, reply.Content, threadOfReply?.Title, forumOfReply?.Name);
            }
            return RedirectToPage(new
            {
                threadID = threadId,
            });
        }
    }
}
