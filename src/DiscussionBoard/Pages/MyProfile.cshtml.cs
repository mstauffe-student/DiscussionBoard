using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DiscussionBoard.Pages
{
    [Authorize]
    public class MyProfile : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserProfileService _userProfileService;
        public ApplicationUser CurrentUser { get; set; }
        [BindProperty] public UserProfile Input { get; set; } = new();
        public string AvatarUrl { get; set; } = ".svg";

        [BindProperty] public int SelectedAvatarId { get; set; }

        public List<SelectListItem> Avatars { get; set; }


        /**
         * Allows injection for user manager and user profile service
         /// <param name="userManager">Service to manage users</param>
         /// <param name="userProfileService">Service to manage user profiles</param>
         */
        public MyProfile(UserManager<ApplicationUser> userManager, IUserProfileService userProfileService)
        {
            _userManager = userManager;
            _userProfileService = userProfileService;
        }

        /**
         * Checks if user is authenticated. Sets up Input for the Userprofile Helper class
         */
        public async Task<IActionResult> OnGet()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
            }

            Input = new UserProfile()
            {
                DisplayName = CurrentUser.DisplayName,
                AvatarId = CurrentUser.AvatarId,
            };

            return Page();
        }

        /**
         * Checks if user is authenticated. Makes sure that the avatar and display name is valid, then
         * assigns the new display name and avatar to the user on submit.
         */
        public async Task<IActionResult> OnPostAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);

            if (CurrentUser == null)
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Page();
            }

            try
            {
                Boolean valid = AvatarCatalog.IsValid(Input.AvatarId);
                if (!valid)
                {
                    TempData["ErrorMessage"] = "Sorry, but that avatar was invalid.";
                    return Page();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["ErrorMessage"] = "Sorry, an error happened with that avatar. Please try again.";
                throw;
            }

            if (String.IsNullOrEmpty(Input.DisplayName) || Input.DisplayName.Length < 2)
            {
                TempData["ErrorMessage"] = "Sorry, but display name can't be empty or less than 2 characters.";
                return Page();
            }

            if (Input.DisplayName.Length > 60)
            {
                TempData["ErrorMessage"] = "Sorry, but display name can't be longer than 60 characters.";
                return Page();
            }
            else
            {
                Input.DisplayName = Input.DisplayName.Trim();
            }

            try
            {
                Boolean valid = _userProfileService.IsDisplayNameAvailableAsync
                    (Input.DisplayName, CurrentUser.Id).Result;
                if (!valid)
                {
                    TempData["ErrorMessage"] = "Sorry, but that display name is taken.";
                    return Page();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["Page"] = "Sorry, an error happened with that display name. Please try again.";
                throw;
            }

            CurrentUser.AvatarId = Input.AvatarId;
            CurrentUser.DisplayName = Input.DisplayName;

            try
            {
                var result = await _userManager.UpdateAsync(CurrentUser);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = "Sorry, but something went wrong. Your avatar could not be updated.";
                    return Page();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["ErrorMessage"] = "Sorry, an error happened. Your avatar could not be updated.";
                throw;
            }

            TempData["Message"] = "Your profile was successfully updated!";
            return RedirectToPage();
        }
    }
}
