using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages
{
    public class Details : PageModel
    {
        private readonly ILogger<Details> _logger;
        private readonly IDiscussionThreadService _threads;
        public List<DiscussionThread> Items { get; private set; } = [];
        public int ForumId {get; private set;}

        /// <summary>
        /// The Forum/Details page, displaying all active threads from a given forum
        /// </summary>
        /// <param name="threads">Service to manage threads</param>
        /// <param name="logger">Logging provider</param>
        public Details(IDiscussionThreadService threads, ILogger<Details> logger)
        {
            _threads = threads;
            _logger  = logger;
        }

        /// <summary>
        /// Returns a list of the active threads of selected forum on the site
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Items = await _threads.GetForForumAsync(id);
                ForumId = id;
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Sorry, but something went wrong." + e.Message;
                _logger.LogError($"Items failure in Forum/Details. {e.Message}");
                throw;
            }
            if (Items == null || Items.Count == 0)
            {
                ViewData["IfEmpty"] = "No threads have been made in this forum yet.";
            }
            return Page();
        }
    }
}

