using System.Security.Claims;
using DiscussionBoard.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class ThreadEditModel : PageModel
    {
        private readonly IDiscussionThreadService _threads;
        private readonly ILogger<ThreadEditModel> _logger;
        private readonly IApplicationAuthorizationService _auth;
        [BindProperty]
        public DiscussionEditViewModel Input { get; set; } = new();

        public DiscussionBase thread;

        /**
         * default constructor to inject services.
         * <param name="threads">Service to manage threads</param>
         * <param name="logger">Service to manage logging</param>
         * <param name="auth">Service to manage authorization</param>
         */
        public ThreadEditModel(IDiscussionThreadService threads, ILogger<ThreadEditModel> logger, IApplicationAuthorizationService auth)
        {
            _threads = threads;
            _logger = logger;
            _auth = auth;
        }

        /**
         * Loads the thread, checks if user is authorized, sets up DiscussionEditViewModel input.
         * <param name="threadId">id of the thread</param>
         */
        public async Task<IActionResult> OnGetAsync(int threadId)
        {
             thread = null;
            try
            {
                thread = await _threads.GetAsync(threadId); //Loads thread to be edited
                if (thread == null)
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var user = User; //Makes sure the active user is valid
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }

            var isAuthorized = await _auth.CanEditAsync(user, thread); //Makes sure the active user is allowed to edit the thread
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that thread.";
                return Forbid();
            }

            return Page();

        }

        /**
         * Takes a threadId for the thread to be edited, makes sure user is authorized and allowed to edit the
         * thread, makes sure the title and content for the new thread is valid, updates the thread if everything is
         * valid.
         * <param name="threadId">id of the thread</param>
         */
        public async Task<IActionResult> OnPostAsync(int threadId)
        {
            thread = null;
            try
            {
                thread = await _threads.GetAsync(threadId); //Loads thread again to make sure it's still ok
                if (thread == null)
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var validUser = User; //Makes sure the active user is valid again
            if (validUser == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }
            var isAuthorized = await _auth.CanEditAsync(validUser, thread); //Makes sure the active user is allowed to edit the thread
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that thread.";
                return Forbid();
            }

            if (string.IsNullOrEmpty(Input.Content))
            {
                //Makes sure content is not null.
                TempData["ErrorMessage"] = "Content can't be empty or less than 2 characters.";
                _logger.LogWarning($"Content {Input.Content} can't be empty or less than 1 characters.");
                return Page();
            }
            if (string.IsNullOrEmpty(Input.Title))
            {
                //Makes sure the title has a name.
                TempData["ErrorMessage"] = "Title can't be empty or less than 2 characters.";
                _logger.LogWarning($"Title {Input.Title} can't be empty or less than 3 characters.");
                return Page();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //validates user again to get their id for the update
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }

            DiscussionThread updatedthread = new DiscussionThread
            {
                Title = Input.Title,
                Content = Input.Content,
                ApplicationUserId = userId,
                Id = threadId
            };
            try
            {
                await _threads.UpdateAsync(updatedthread);
            }
            catch (Exception e)
            {
                //Hopefully catches if there's a database issue
                Console.WriteLine(e.Message);
                TempData["ErrorMessage"] = "Something went wrong. Couldn't edit this. " + e.Message;
                _logger.LogError($"Could not edit the thread {thread} to thread {updatedthread}. {e.Message}");
            }
            return RedirectToPage();
        }
    }
}

