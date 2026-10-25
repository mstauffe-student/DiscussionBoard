using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    public class Edit : PageModel
    {
        [BindProperty]
        public UserInputModel Input { get; set; } = new();
        private readonly IDiscussionThreadService _threads;
        public List<DiscussionThread> Items { get; private set; } = new();
        private readonly ILogger<Edit> _logger;

        public Edit(IDiscussionThreadService threads, ILogger<Edit> logger)
        {
            _threads = threads;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //Lines 34-49 makes sure the name of the thread to be edited exists. It gets the id of the thread to be
            //changed.
            int id = -1;
            Items = await _threads.GetAllAsync();
            foreach (var t in Items)
            {
                if (t.Title == Input.ThreadName)
                {
                    id = t.Id;
                }
            }
            if (id == -1)
            {
                TempData["ErrorMessage"] = $"Unable to find a thread with that name {Input.ThreadName}. ";
                _logger.LogWarning($"Unable to find a thread with that name {Input.ThreadName}.");
                return RedirectToPage();
            }

            if (Input.Content == null || Input.Content == string.Empty)
            {
                //Makes sure content is not null.
                Input.Content = "";
            }
            if (Input.Title == null || Input.Title == string.Empty)
            {
                //Makes sure the title has a name.
                TempData["ErrorMessage"] = "Title and content can't be empty or less than 2 characters.";
                _logger.LogWarning($"Title {Input.Title} can't be empty or less than 2 characters.");
                return RedirectToPage();
            }

            if (user == null)
            {
                //Stops an authorized user from making a post.
                TempData["ErrorMessage"] = "Sorry, but you are not authorized.";
                _logger.LogWarning($"User {user} is unauthorized and tried to create a post.");
                return RedirectToPage();
            }
            else
            {
                DiscussionThread thread = new DiscussionThread
                {
                    Title = Input.Title,
                    Content = Input.Content,
                    ApplicationUserId = user,
                    Id = id
                };
                try
                {
                    await _threads.UpdateAsync(thread);
                }
                catch (Exception e)
                {
                    //Hopefully catches if there's a database issue
                    Console.WriteLine(e.Message);
                    TempData["ErrorMessage"] = "Something went wrong. Couldn't edit this. " + e.Message;
                    _logger.LogError($"Could not edit the thread {Input.ThreadName} to thread {thread}. {e.Message}");
                }
            }
            return RedirectToPage("./Edit");
        }
    }
}

