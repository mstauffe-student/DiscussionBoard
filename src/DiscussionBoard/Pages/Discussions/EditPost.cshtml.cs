using System.Security.Claims;
using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class EditPostModel : PageModel
    {
        private readonly IDiscussionThreadService _threads;
        private readonly ILogger<EditPostModel> _logger;
        private readonly IApplicationAuthorizationService _auth;
        private readonly IPostService _posts;
        [BindProperty]
        public DiscussionEditViewModel Input { get; set; } = new();

        public DiscussionBase thread;
        public Post post;

        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUser CurrentUser;

        /// <summary>
        /// default constructor to inject services.
        /// </summary>
        /// <param name="threads">Service to manage threads</param>
        /// <param name="logger">Logging provider</param>
        /// <param name="userManager">Service to manage users</param>
        /// <param name="posts">Service to manage posts</param>
        /// <param name="auth">Service to manage authorization</param>
        public EditPostModel(IDiscussionThreadService threads, IApplicationAuthorizationService auth, ILogger<EditPostModel> logger
        , IPostService posts, UserManager<ApplicationUser> userManager)
        {
            _threads = threads;
            _auth = auth;
            _logger = logger;
            _posts = posts;
            _userManager = userManager;
        }

        /// <summary>
        /// Loads post and makes sure its valid, checks if user is authorized, loads input.
        /// </summary>
        ///<param name="threadId">id of the thread</param>
        /// <param name="postId">id of the post</param>
        public async Task<IActionResult> OnGetAsync(int threadId, int postId)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) //Sets up for logging
            {
                //Stops an authenticated user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authenticated!";
                _logger.LogWarning("An authenticated user tried to edit a thread!");
                return RedirectToPage();
            }
            if (string.IsNullOrEmpty(CurrentUser.DisplayName))
            {
                CurrentUser.DisplayName = "Unconfigured User";
            }

            thread = null;
            post = null;
            try
            {
                thread = await _threads.GetAsync(threadId);
                post = await _posts.GetAsync(postId);
                if (post == null || post.DiscussionThreadId != threadId || thread == null) //makes sure post and thread are still valid
                {
                    _logger.LogWarning("User '{CurrentUser}' tried to edit a post but the thread or post was not valid!",
                        CurrentUser.DisplayName);
                    return NotFound();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            DiscussionThread threadOfPostEdit = null; //for logging
            Forum forumOfPostEdit = null; //for logging
            try
            {
                threadOfPostEdit = await _posts.GetPostThreadAsync(threadId); //For logging
                forumOfPostEdit = await _posts.GetPostForumAsync(threadOfPostEdit.ForumId); //For logging
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Could not load thread or forum for logging.");
                throw;
            }

            var user = User; //makes sure user is valid
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                _logger.LogWarning("An authenticated user tried to edit a post {post}! Within thread '{thread}' in forum '{forum}'",
                    post.Content, threadOfPostEdit?.Title, forumOfPostEdit?.Name);
                return Forbid();
            }

            var isAuthorized = await _auth.CanEditAsync(user, post); //makes sure user is allowed to edit the post
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that post.";
                _logger.LogWarning("User '{User}' tried to edit a post but is not authorized to edit that post {post}! Within thread '{thread}' in forum '{forum}'",
                   post.Content, CurrentUser.DisplayName, threadOfPostEdit.Title, forumOfPostEdit.Name);
                return Forbid();
            }

            Input = new DiscussionEditViewModel //binds Input
            {
                Title = "Edited Title", //Posts don't really have a title
            };
            return Page();
        }

        /// <summary>
        /// Loads post and makes sure its valid, makes sure user is valid, makes sure new content for post is valid,
        /// updates the post is everything is valid.
        /// </summary>
        ///<param name="threadId">id of the thread</param>
        /// <param name="postId">id of the post</param>
        public async Task<IActionResult> OnPostAsync(int threadId, int postId)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) //Sets up for logging
            {
                //Stops an authenticated user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authenticated!";
                _logger.LogWarning("An authenticated user tried to edit a thread!");
                return RedirectToPage();
            }
            if (string.IsNullOrEmpty(CurrentUser.DisplayName))
            {
                CurrentUser.DisplayName = "Unconfigured User";
            }

            thread = null;
            post = null;
            try
            {
                thread = await _threads.GetAsync(threadId);
                post = await _posts.GetAsync(postId);
                if (post == null || post.DiscussionThreadId != threadId || thread == null) //makes sure post and thread are still valid
                {
                    _logger.LogWarning("User '{CurrentUser}' tried to edit a post but the thread or post was not valid!",
                        CurrentUser.DisplayName);
                    return NotFound();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            DiscussionThread threadOfPostEdit = null; //for logging
            Forum forumOfPostEdit = null; //for logging
            try
            {
                threadOfPostEdit = await _posts.GetPostThreadAsync(threadId); //For logging
                forumOfPostEdit = await _posts.GetPostForumAsync(threadOfPostEdit.ForumId); //For logging
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Could not load thread or forum for logging.");
                throw;
            }

            var user = User; //makes sure user is valid again
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                _logger.LogWarning("An authenticated user tried to edit a post {post}! Within thread '{thread}' in forum '{forum}'",
                    post.Content, threadOfPostEdit?.Title, forumOfPostEdit?.Name);
                return Forbid();
            }

            var isAuthorized = await _auth.CanEditAsync(user, post); //makes sure user is allowed to edit post again
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that post.";
                _logger.LogWarning("User '{User}' tried to edit a thread but is not authorized to edit that post {post}! Within thread '{thread}' in forum '{forum}'",
                    post.Content, CurrentUser.DisplayName, threadOfPostEdit?.Title, forumOfPostEdit?.Name);
                return Forbid();
            }

            if (string.IsNullOrEmpty(Input.Content))
            {
                //Makes sure content is not null.
                TempData["ErrorMessage"] = "Content can't be empty or less than 2 characters.";
                _logger.LogWarning("User '{CurrentUser}' tried to edit post '{post}' to new post '{NewPost}' in '{thread}' in '{forum}', but it was empty or less than 2 characters.",
                    CurrentUser.DisplayName, post.Content, Input.Content,  threadOfPostEdit?.Content, forumOfPostEdit?.Name);
                return Page();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //validates user for their id for the update
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                _logger.LogWarning("An authenticated user tried to edit a post! Within thread '{thread}' in forum '{forum}'",
                    threadOfPostEdit?.Title, forumOfPostEdit?.Name);
                return Forbid();
            }
            string oldPost = post.Content;
            Post updatedPost = new Post
            {
                Title = "Edited title", //Posts dont really have titles
                Content = Input.Content,
                ApplicationUserId = userId,
                Id = postId,
                DiscussionThreadId = threadId,

            };
            try
            {
                await _posts.UpdateAsync(updatedPost);
            }
            catch (Exception e)
            {
                //Hopefully catches if there's a database issue
                Console.WriteLine(e.Message);
                TempData["ErrorMessage"] = "Something went wrong. Couldn't edit this. " + e.Message;
                _logger.LogError("Could not add reply! User '{User}' failed to edit post '{post}' to '{Content}' in thread '{thread}' in forum '{forum}'. {ErrorMessage}"
                    ,CurrentUser.DisplayName, oldPost, updatedPost?.Content, threadOfPostEdit?.Title, forumOfPostEdit?.Name, e.Message);
                return RedirectToPage();
            }
            _logger.LogInformation("User '{User}' edited post '{post}' to '{Content}' in thread '{thread}' in forum '{forum}'.",
                CurrentUser.DisplayName, oldPost, updatedPost?.Content, threadOfPostEdit?.Title, forumOfPostEdit?.Name);
            return RedirectToPage();
        }
    }
}
