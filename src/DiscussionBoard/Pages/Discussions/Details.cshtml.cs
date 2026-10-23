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
        public DiscussionThread Thread;
        public string ErrorMessage { get; set; } = string.Empty;
        public UserInputModel Input { get; set; } = new UserInputModel();

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
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Sorry, but something went wrong.";
            }
            try
            {
                ReplyItems = PostTreeBuilder.Build(await _post.GetForThreadAsync(id));
            }
            catch (Exception e)
            {
                ErrorMessage = "Sorry, but something went wrong." + e.Message;
                _logger.LogError($"ReplyItems failure in Discussions/Details. {e.Message}");
                throw;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Sorry, but something went wrong.";
                return Page();
            }

            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Post post = new Post()
            {
                Title = Input.Title,
                Content = Input.Content,
                ApplicationUserId = user,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (user == null)
            {
                ErrorMessage = "Sorry, but you are not authorized.";
                return Page();
            }
            else
            {
                try
                {
                    await _post.AddAsync(post);
                    ReplyItems = PostTreeBuilder.Build(await _post.GetForThreadAsync(id));
                }
                catch (Exception e)
                {
                    ErrorMessage = "Something went wrong. Couldn't add this. " + e.Message;
                }
            }
            return Page();
        }

    }
}
