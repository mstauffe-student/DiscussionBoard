using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DiscussionBoard.Models;

namespace DiscussionBoard.Pages;

public class ForumsModel : PageModel
{
    private readonly ILogger<ForumsModel> _logger;
    private readonly IForumService _forums;

    public List<Forum> Items { get; private set; } = new();

    /// <summary>
    /// The Forums page, displaying all active forums
    /// </summary>
    /// <param name="forums">Service to manage forums</param>
    /// <param name="logger">Logging provider</param>
    public ForumsModel(IForumService forums, ILogger<ForumsModel> logger)
    {
        _forums = forums;
        _logger = logger;
    }

    /// <summary>
    /// Returns a list of the active forums on the site
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Items = await _forums.GetAllAsync();
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = "Sorry, but something went wrong." + e.Message;
            _logger.LogError($"Items failure in Forum/Index. {e.Message}");
            throw;
        }
        if (Items == null || Items.Count == 0)
        {
            ViewData["IfEmpty"] = "No forums have been made.";
        }
        return Page();
    }
}
