using System.Security.Claims;
using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ValidateAntiForgeryToken]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IApplicationAuthorizationService  _authorizationService;
        private readonly ILogger<UsersModel> _logger;
        public ClaimsPrincipal CurrentUser { get; set; }
        [BindProperty]
        public UserModelHelper Input { get; set; } = new();
        public bool IsAdmin { get; set; }
        [BindProperty]
        public List<ApplicationUser> Users { get; private set; }
        public ApplicationUser SameUser { get; set; }
        [BindProperty]
        public string SelectedUserId { get; set; }

        /**
         * default constructor to inject services.
         * <param name="userManager">Service to manage users</param>
         * <param name="authorizationService">Service to manage authorization</param>
         * <param name="logger">Service to manage logs</param>
         * <param name="roleManager">Service to manage roles</param>
         */
        public UsersModel(UserManager<ApplicationUser> userManager,  RoleManager<IdentityRole> roleManager, IApplicationAuthorizationService authorizationService,
            ILogger<UsersModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _authorizationService = authorizationService;
        }

        /**
         * Loads a list of users, makes sure the active user is valid, binds the Input.
         */
        public async Task<IActionResult> OnGet()
        {
            CurrentUser = User;
            if (CurrentUser == null) //makes sure active user claims principle is valid
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }
            try
            {
                Users = await _userManager.Users.ToListAsync();
                SameUser = await _userManager.GetUserAsync(User);
                if (SameUser == null) //makes active user is valid
                {
                    TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                    return Page();
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Sorry, something went wrong loading the users.";
                return Page();
            }

            Input = new Models.UserModelHelper() //binds Input
            {
                SelectedUserId = SelectedUserId,
                ChosenUser = ChosenUser,
                IsAdmin = IsAdmin

            };
            return Page();
        }

        public ApplicationUser ChosenUser { get; set; }

        /**
         * loads chosen user and makes sure they're valid, checks if chosen user is an admin or not, reloads user select list
         */
        public async Task<IActionResult> OnPost()
        {
            var selectedUserId = Input.SelectedUserId;
            try
            {
                Input.ChosenUser = await _userManager.FindByIdAsync(selectedUserId);
                if (Input.ChosenUser == null) //makes sure chosen user is valid
                {
                    TempData["ErrorMessage"] = "Sorry, but that user is null.";
                    return RedirectToPage();
                }
                Input.IsAdmin = await _userManager.IsInRoleAsync(Input.ChosenUser, ApplicationRoles.Admin); //checks if chosen user is an admin
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["ErrorMessage"] = "Sorry, something went wrong with the user you selected.";
                return RedirectToPage();
            }
            try
            {
                Users = await _userManager.Users.ToListAsync(); //reloads user list to prevent crash
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["ErrorMessage"] = "Sorry, something went wrong loading the users.";
                return RedirectToPage();
            }
            TempData["Message"] = "You have successfully chosen a user!";
            return Page();
        }

        /**
         * validates chosen user again and checks if they're in admin, makes sure chosen user isn't the last user or
         * is the active user, validates the active user, adds or removes the chosen user from admin.
         */
        public async Task<IActionResult> OnPostUpdateRoleAsync()
        {
            if (!await _roleManager.RoleExistsAsync(ApplicationRoles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Admin));
            }
            var selectedUserId = Input.SelectedUserId;
            try
            {
                Input.ChosenUser = await _userManager.FindByIdAsync(selectedUserId);
                if (Input.ChosenUser == null) //makes sure chosen user is valid
                {
                    TempData["ErrorMessage"] = "Sorry, but that user is null.";
                    return RedirectToPage();
                }
                Input.IsAdmin = await _userManager.IsInRoleAsync(Input.ChosenUser, ApplicationRoles.Admin); //checks if chosen user is an admin
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["ErrorMessage"] = "Sorry, something went very wrong loading that user.";
                return RedirectToPage();
            }
            CurrentUser = User;
            if (CurrentUser == null) //makes sure active user claims principle is valid
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }
            SameUser = await _userManager.GetUserAsync(User);
            if (SameUser == null) //makes active user is valid
            {
                TempData["ErrorMessage"] = "Sorry, but you're not authenticated.";
                return Forbid();
            }

            IdentityResult result = new IdentityResult();
            if (Input.IsAdmin)
            {
                try
                {
                    var adminUsers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
                    if (adminUsers.Count == 1) //makes sure last admin isnt removed
                    {
                        TempData["ErrorMessage"] = "Sorry, you can't remove the last admin.";
                        return RedirectToPage();
                    }
                    if (SameUser.Id == Input.ChosenUser.Id) //makes sure a user cant remove themselves
                    {
                        TempData["ErrorMessage"] = "Sorry, you can't remove yourself from admin.";
                        return RedirectToPage();
                    }
                    //if the chosen user is an admin, try to remove them:
                     result = await _authorizationService.RemoveRoleAsync(CurrentUser, Input.ChosenUser.Id, ApplicationRoles.Admin);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    TempData["ErrorMessage"] = $"{result} to remove the user from admin. Something went wrong.";
                    return RedirectToPage();
                }
                TempData["Message"] = $"{result} to remove the user from admin!";
            }
            else
            {
                try
                {
                   //if chosen user isn't an admin, try adding them to admin:
                    result = await _authorizationService.AddRoleAsync(CurrentUser, Input.ChosenUser.Id, ApplicationRoles.Admin);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    TempData["ErrorMessage"] = $"{result} to add the user to admin. Something went wrong.";
                    return RedirectToPage();
                }
                TempData["Message"] = $"{result} to add the user to admin!";
            }
            if (result.Succeeded)
            {
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage();
            }
            return Page();
        }
    }
}
