using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiscussionBoard.Authorization
{
    public class ApplicationAuthorizationService : IApplicationAuthorizationService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /**
         * constructor to add injection.
         * <param name="authorizationService">Service to manage posts</param>
         * <param name="userManager">Service to manage posts</param>
         * <param name="roleManager">Service to manage posts</param>
         */
        public ApplicationAuthorizationService(IAuthorizationService authorizationService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _authorizationService = authorizationService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /**
         * Tests to see if a user is allowed to edit something or not.
         * <param name="resource">the discussion resource</param>
         * <param name="actingUser">the user trying to do something</param>
         */
        public async Task<bool> CanEditAsync(ClaimsPrincipal actingUser, DiscussionBase resource)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(actingUser, resource,
                ApplicationAuthorizationPolicies.IsOwnerOrAdmin);
            return authorizationResult.Succeeded;
        }

        /**
         * Validates the user that's making a change. If everything is fine, the targeted user will be added to
         * the role name.
         * <param name="actingUser">the user trying to do something</param>
         * <param name="targetUserId">the id of the user to be added</param>
         * <param name="roleName">the name of the role</param>
         */
        public async Task<IdentityResult> AddRoleAsync(ClaimsPrincipal actingUser, string targetUserId, string roleName)
        {
            if (actingUser == null || actingUser.Identity?.IsAuthenticated != true || !actingUser.IsInRole(ApplicationRoles.Admin))
            {
                return IdentityResult.Failed();
            }

            var targetedUser = await _userManager.FindByIdAsync(targetUserId);

            if (targetedUser == null)
            {
                return IdentityResult.Failed();
            }

            if (roleName == ApplicationRoles.Admin)
            {
                var user = await _userManager.GetUserAsync(actingUser);
                if (user.Id == targetedUser.Id)
                {
                    return IdentityResult.Failed();
                }

                var userResult = await _userManager.AddToRoleAsync(targetedUser, roleName);
                if (userResult.Succeeded)
                {
                    return IdentityResult.Success;
                }
            }
            return IdentityResult.Failed();
        }

        /**
         * Validates the user that's making a change. If everything is fine, the targeted user will be removed from
         * the role name.
         * <param name="actingUser">the user trying to do something</param>
         * <param name="targetUserId">the id of the user to be removed</param>
         * <param name="roleName">the name of the role</param>
         */
        public async Task<IdentityResult> RemoveRoleAsync(ClaimsPrincipal actingUser,string targetUserId, string roleName)
        {
            if (actingUser == null || actingUser.Identity?.IsAuthenticated != true)
            {
                return IdentityResult.Failed();
            }

            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(roleName))
            {
                return IdentityResult.Failed();
            }

            var targetedUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetedUser == null)
            {
                return IdentityResult.Failed();
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return IdentityResult.Failed();
            }

            if (roleName == ApplicationRoles.Admin)
            {
                var user = await _userManager.GetUserAsync(actingUser);
                if (user.Id == targetedUser.Id || !actingUser.IsInRole(ApplicationRoles.Admin))
                {
                    return IdentityResult.Failed();
                }

                var adminUsers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
                if (adminUsers.Count == 1 && adminUsers[0].Id == targetedUser.Id)
                {
                    return IdentityResult.Failed();
                }

                var removeResult = await _userManager.RemoveFromRoleAsync(targetedUser, ApplicationRoles.Admin);
                if (removeResult.Succeeded)
                {
                    return IdentityResult.Success;
                }
            }
            return IdentityResult.Failed();
        }

    }
}
