using System.Security.Claims;
using DiscussionBoard.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
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

        public DiscussionBase? thread;

        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUser? CurrentUser;

        /// <summary>
        /// default constructor to inject services.
        /// </summary>
        /// <param name="threads">Service to manage threads</param>
        /// <param name="logger">Logging provider</param>
        /// <param name="userManager">Service to manage users</param>
        /// <param name="auth">Service to manage authorization</param>
        public ThreadEditModel(IDiscussionThreadService threads, ILogger<ThreadEditModel> logger, IApplicationAuthorizationService auth,
            UserManager<ApplicationUser> userManager)
        {
            _threads = threads;
            _logger = logger;
            _auth = auth;
            _userManager = userManager;
        }

        /// <summary>
        /// default constructor to inject services.
        /// </summary>
        /// <param name="threadId">id of the thread</param>
        public async Task<IActionResult> OnGetAsync(int threadId)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) //Sets up for logging
            {
                //Stops an unauthorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized!";
                _logger.LogWarning("An unauthorized user tried to edit a thread!");
                return RedirectToPage();
            }
            if (string.IsNullOrEmpty(CurrentUser.DisplayName))
            {
                CurrentUser.DisplayName = "Unconfigured User";
            }

            thread = null;
            try
            {
                thread = await _threads.GetAsync(threadId); //Loads thread to be edited
                if (thread == null)
                {
                    _logger.LogWarning("User '{User}' tried to edit a thread but the thread was not valid!",
                        CurrentUser.DisplayName);
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            DiscussionThread? forumthreadId = null; //logging
            Forum? forumOfThread = null; //for logging
            try
            {
                forumthreadId = await _threads.GetAsync(threadId);
                forumOfThread = await _threads.GetThreadForumAsync(forumthreadId.ForumId); //For logging
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Could not load thread or forum for logging.");
                throw;
            }

            var user = User; //Makes sure the active user is valid
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authorized.";
                _logger.LogWarning("An unauthorized user tried to edit a thread '{thread}'! Within in forum '{forum}'",
                    thread.Title, forumOfThread?.Name);
                return Forbid();
            }

            var isAuthorized = await _auth.CanEditAsync(user, thread); //Makes sure the active user is allowed to edit the thread
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that thread.";
                _logger.LogWarning("User '{User}' tried to edit a thread but is not authorized to edit that thread '{thread}'! Within forum '{forum}'",
                     CurrentUser.DisplayName, thread.Title, forumOfThread.Name);
                return Forbid();
            }

            return Page();

        }

        /// <summary>
        /// Takes a threadId for the thread to be edited, makes sure user is authorized and allowed to edit the
        /// thread, makes sure the title and content for the new thread is valid, updates the thread if everything is
        /// valid.
        /// </summary>
        /// <param name="threadId">id of the thread</param>
        public async Task<IActionResult> OnPostAsync(int threadId)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) //Sets up for logging
            {
                //Stops an unauthorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized!";
                _logger.LogWarning("An unauthorized user tried to edit a thread!");
                return RedirectToPage();
            }
            if (string.IsNullOrEmpty(CurrentUser.DisplayName))
            {
                CurrentUser.DisplayName = "Unconfigured User";
            }

            thread = null;
            try
            {
                thread = await _threads.GetAsync(threadId); //Loads thread again to make sure it's still ok
                if (thread == null)
                {
                    _logger.LogWarning("User '{user}' tried to edit a thread but the thread was not valid!",
                        CurrentUser.DisplayName);
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            DiscussionThread? forumthreadId = null; //logging
            Forum? forumOfThread = null; //for logging
            try
            {
                forumthreadId = await _threads.GetAsync(threadId);
                forumOfThread = await _threads.GetThreadForumAsync(forumthreadId.ForumId); //For logging
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Could not load thread or forum for logging.");
                throw;
            }

            var validUser = User; //Makes sure the active user is valid again
            if (validUser == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authorized.";
                _logger.LogWarning("An unauthorized user tried to edit a thread '{thread}'! Within in forum '{forum}'",
                    thread.Title, forumOfThread?.Name);
                return Forbid();
            }
            var isAuthorized = await _auth.CanEditAsync(validUser, thread); //Makes sure the active user is allowed to edit the thread
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that thread.";
                _logger.LogWarning("User '{User}' tried to edit a thread but is not authorized to edit that thread '{thread}'! Within forum '{forum}'",
                    CurrentUser.DisplayName, thread.Title, forumOfThread.Name);
                return Forbid();
            }

            if (string.IsNullOrEmpty(Input.Content))
            {
                //Makes sure content is not null.
                TempData["ErrorMessage"] = "Content can't be empty or less than 2 characters.";
                _logger.LogWarning("User '{User}' tried to edit thread content '{old}' to new thread content '{new}' in forum '{forum}', but content was empty or less than 2 characters.",
                    CurrentUser.DisplayName, thread.Content, Input.Content, forumOfThread?.Name);
                return Page();
            }
            if (string.IsNullOrEmpty(Input.Title))
            {
                //Makes sure the title has a name.
                TempData["ErrorMessage"] = "Title can't be empty or less than 2 characters.";
                _logger.LogWarning("User '{User}' tried to edit thread title '{old}' to new thread title '{new}' in forum '{forum}', but title was empty or less than 2 characters.",
                    CurrentUser.DisplayName, thread.Title, Input.Title, forumOfThread?.Name);
                return Page();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //validates user again to get their id for the update
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authorized.";
                _logger.LogWarning("An unauthorized user tried to edit a thread '{thread}'! Within in forum '{forum}'",
                    thread.Title, forumOfThread?.Name);
                return Forbid();
            }

            string oldThreadTitle = thread.Title;
            string? oldThreadContent = thread.Content;
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
                _logger.LogError("Could not edit thread! User '{User}' failed to edit thread title '{OldTitle}' and content '{OldContent}' to title '{Title}' and content '{Content}' in forum '{forum}'. {ErrorMessage}"
                    ,CurrentUser.DisplayName, oldThreadTitle, oldThreadContent, updatedthread?.Title, updatedthread?.Content, forumOfThread?.Name, e.Message);
                return RedirectToPage();
            }
            _logger.LogInformation("User '{User}' edited thread title '{OldTitle}' and content '{OldContent}' to title '{Title}' and content '{Content}' in forum '{forum}'"
                ,CurrentUser.DisplayName, oldThreadTitle, oldThreadContent, updatedthread?.Title, updatedthread?.Content, forumOfThread?.Name);
            return RedirectToPage();
        }
    }
}

