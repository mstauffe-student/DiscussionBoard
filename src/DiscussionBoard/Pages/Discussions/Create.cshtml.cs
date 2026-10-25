using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.Pages.Forums;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    public class Create : PageModel
    {
        [BindProperty]
        public UserInputModel Input { get; set; } = new();
        private readonly IDiscussionThreadService _threads;
        private readonly ILogger<Create> _logger;


        /// <summary>
        /// The Create page, allowing for a thread to be added.
        /// </summary>
        /// <param name="threads">Service to manage threads</param>
        /// <param name="logger">Logging provider</param>
        public Create(IDiscussionThreadService threads, ILogger<Create> logger)
        {
            _threads = threads;
            _logger = logger;
        }

        /**
         * Allows a user to add a thread to a forum. Users can only add title and content.
         * <param name="id">id of the thread</param>
         * <param name="title">thread title to be added</param>
         * <param name="content">thread content to be added</param>
         */
        public async Task<IActionResult> OnPostAsync(int id, string title, string content)
        {
            if (content == null || content == string.Empty)
            {
                //Makes sure content is not null.
                content = "";
            }
            if (title == null || title == string.Empty)
            {
                //Makes sure the title has a name.
                TempData["ErrorMessage"] = "Title and content can't be empty or less than 2 characters.";
                _logger.LogWarning($"Title {title} can't be empty or less than 2 characters.");
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
                Input.Title  = title;
                Input.Content = content;

                DiscussionThread thread = new DiscussionThread
                {
                    Title = Input.Title,
                    Content = Input.Content,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ForumId = id,
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
                    TempData["ErrorMessage"] = "Something went wrong. Couldn't add this. " + e.Message;
                    _logger.LogError($"Could not add the thread {thread}. {e.Message}");
                }
            }
            return RedirectToPage();
        }
    }
}
