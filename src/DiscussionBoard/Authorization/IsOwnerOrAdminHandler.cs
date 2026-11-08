using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscussionBoard.Authorization
{
    public class IsOwnerOrAdminHandler : AuthorizationHandler<IsOwnerOrAdminRequirement, DiscussionBase>
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            IsOwnerOrAdminRequirement requirement, DiscussionBase resource)
        {
            if (context.User.IsInRole(ApplicationRoles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null && resource.ApplicationUserId == userId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

    }
}
