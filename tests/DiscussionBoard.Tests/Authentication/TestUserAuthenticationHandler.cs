using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscussionBoard.Tests.Authentication;

/// <summary>Only requests carrying the test header are authenticated.</summary>
public sealed class TestUserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "IntegrationTestUser";
    public const string UserHeader = "X-Integration-Test-User-Id";
    public const string RoleHeader = "X-Integration-Test-User-Role"; //I had to add this to get tests to work

    public TestUserAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var userIds) ||
            string.IsNullOrWhiteSpace(userIds.ToString()))
            return Task.FromResult(AuthenticateResult.NoResult());

        var id = userIds.ToString();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Name, id),
        }, Scheme);

        //I had to add lines 33-39 to get tests to work otherwise the admin role wouldn't get added to the test admin users
        if (Request.Headers.TryGetValue(TestUserAuthenticationHandler.RoleHeader, out var roles))
        {
            foreach (var role in roles.ToString().Split(','))
            {
                if (!string.IsNullOrWhiteSpace(role))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.Trim()));;
                }
            }
        }
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme)));
    }
}
