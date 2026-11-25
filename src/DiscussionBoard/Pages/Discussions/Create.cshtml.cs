using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.Pages.Forums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class Create : PageModel
    {
        [BindProperty]
        public UserInputModel Input { get; set; } = new();
        private readonly IDiscussionThreadService _threads;
        private readonly IForumService _forums;
        private readonly ILogger<Create> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUser? CurrentUser;


        /// <summary>
        /// default constructor to inject services.
        /// </summary>
        /// <param name="threads">Service to manage threads</param>
        /// <param name="logger">Logging provider</param>
        /// <param name="userManager">Service to manage users</param>
        /// <param name="forums">Service to manage forums</param>
        public Create(IDiscussionThreadService threads, ILogger<Create> logger, UserManager<ApplicationUser> userManager,
            IForumService forums)
        {
            _threads = threads;
            _logger = logger;
            _userManager = userManager;
            _forums = forums;
        }

        /// <summary>
        /// Allows a user to add a thread to a forum. Users can only add title and content.
        /// </summary>
        /// <param name="threadId">id of the thread</param>
        /// <param name="title">thread title to be added</param>
        /// <param name="content">thread content to be added</param>
        public async Task<IActionResult> OnPostAsync(int threadId, string title, string content)
        {
            Forum? forumOfThread = null; //for logging
            try
            {
                forumOfThread = await _forums.GetAsync(threadId); //For logging
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
                _logger.LogWarning("An unauthorized user tried to create a thread! Within forum '{forum}'",
                    forumOfThread?.Name);
                return RedirectToPage();
            }
            if (string.IsNullOrEmpty(CurrentUser.DisplayName))
            {
                CurrentUser.DisplayName = "Unconfigured User";
            }

            if (string.IsNullOrEmpty(content))
            {
                //Makes sure content is not null.
                content = "";
            }
            if (string.IsNullOrEmpty(title))
            {
                //Makes sure the title has a name.
                TempData["ErrorMessage"] = "Title and content can't be empty or less than 2 characters.";
                _logger.LogWarning("User '{User}' tried to create thread title '{title}' in forum '{forum}' that's empty or less than 2 characters.",
                    CurrentUser.DisplayName, title, forumOfThread?.Name);
                return RedirectToPage();
            }

            string? user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                //Stops an unauthorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized!";
                _logger.LogWarning("An unauthorized user tried to create a thread! Within forum '{forum}'",
                    forumOfThread?.Name);
                return RedirectToPage();
            }
            else
            {
                Input.Title  = title;
                Input.Content = content;
                DiscussionThread thread = new DiscussionThread
                {
                    Title = Input.Title,
                    Content = Input.Content,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ForumId = threadId,
                    ApplicationUserId = user,
                    IsDeleted = false
                };
                try
                {
                    await _threads.AddAsync(thread);
                }
                catch (Exception e)
                {
                    //Hopefully catches if there's a database issue
                    Console.WriteLine(e.Message);
                    TempData["ErrorMessage"] = "Something went wrong. Couldn't add this thread. " + e.Message;
                    _logger.LogError("Could not add thread! User '{User}' failed to add thread with title '{Title}' and content '{Content}' in forum '{forum}'. {ErrorMessage}"
                        ,CurrentUser.DisplayName, title, content, forumOfThread?.Name, e.Message);
                    return RedirectToPage();
                }
                _logger.LogInformation("User '{User}' created thread with title '{Title}' and content '{Content}' in forum '{forum}'.",
                    CurrentUser.DisplayName, title, content, forumOfThread?.Name);
            }
            return RedirectToPage();
        }
    }
}
