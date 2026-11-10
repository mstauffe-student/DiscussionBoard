using System.Security.Claims;
using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class EditPostModel : PageModel
    {
        private readonly IDiscussionThreadService _threads;
        private readonly ILogger<ThreadEditModel> _logger;
        private readonly IApplicationAuthorizationService _auth;
        private readonly IPostService _posts;
        [BindProperty]
        public DiscussionEditViewModel Input { get; set; } = new();

        public DiscussionBase thread;
        public Post post;

        /**
         * default constructor to inject services.
         * <param name="threads">Service to manage threads</param>
         * <param name="auth">Service to manage authorization</param>
         * <param name="logger">Service to manage logs</param>
         * <param name="posts">Service to manage posts</param>
         */
        public EditPostModel(IDiscussionThreadService threads, IApplicationAuthorizationService auth, ILogger<ThreadEditModel> logger
        , IPostService posts)
        {
            _threads = threads;
            _auth = auth;
            _logger = logger;
            _posts = posts;
        }

        /**
         * Loads post and makes sure its valid, checks if user is authorized, loads input.
         * <param name="threadId">id of the thread</param>
         * <param name="postId">id of the post</param>
         */
        public async Task<IActionResult> OnGetAsync(int threadId, int postId)
        {
            thread = null;
            post = null;
            try
            {
                thread = await _threads.GetAsync(threadId);
                post = await _posts.GetAsync(postId);
                if (post == null || post.DiscussionThreadId != threadId || thread == null) //makes sure post and thread are valid
                {
                    return NotFound();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var user = User; //makes sure user is valid
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }

            var isAuthorized = await _auth.CanEditAsync(user, post); //makes sure user is allowed to edit the post
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that post.";
                return Forbid();
            }

            Input = new DiscussionEditViewModel //binds Input
            {
                Title = "Edited Title", //Posts dont really have a title
            };
            return Page();
        }

        /**
         * Loads post and makes sure its valid, makes sure user is valid, makes sure new content for post is valid,
         * updates the post is everything is valid.
         * <param name="threadId">id of the thread</param>
         * <param name="postId">id of the post</param>
         */
        public async Task<IActionResult> OnPostAsync(int threadId, int postId)
        {
            thread = null;
            post = null;
            try
            {
                thread = await _threads.GetAsync(threadId);
                post = await _posts.GetAsync(postId);
                if (post == null || post.DiscussionThreadId != threadId || thread == null) //makes sure post and thread are still valid
                {
                    return NotFound();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var user = User; //makes sure user is valid again
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }

            var isAuthorized = await _auth.CanEditAsync(user, post); //makes sure user is allowed to edit post again
            if (!isAuthorized)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit that post.";
                return Forbid();
            }

            if (string.IsNullOrEmpty(Input.Content))
            {
                //Makes sure content is not null.
                TempData["ErrorMessage"] = "Content can't be empty or less than 2 characters.";
                _logger.LogWarning($"Content {Input.Content} can't be empty or less than 1 characters.");
                return Page();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //validates user for their id for the update
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }

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
                _logger.LogError($"Could not edit the thread {post} to thread {updatedPost}. {e.Message}");
            }
            return RedirectToPage();
        }
    }
}
