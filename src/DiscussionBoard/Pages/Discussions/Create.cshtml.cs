using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
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
        public string ErrorMessage { get; set; } = string.Empty;

        public Create(IDiscussionThreadService threads)
        {
            _threads = threads;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DiscussionThread thread = new DiscussionThread
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
                    await _threads.AddAsync(thread);
                }
                catch (Exception e)
                {
                    ErrorMessage = "Something went wrong. Couldn't add this. " + e.Message;
                }
            }
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Sorry, but something went wrong.";
                return Page();
            }
            return RedirectToPage("./Create");
        }
    }
}
