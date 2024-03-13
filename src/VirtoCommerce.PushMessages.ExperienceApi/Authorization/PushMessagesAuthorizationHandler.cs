using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace VirtoCommerce.PushMessages.ExperienceApi.Authorization;

public class PushMessagesAuthorizationRequirement : IAuthorizationRequirement
{
}

public class PushMessagesAuthorizationHandler : AuthorizationHandler<PushMessagesAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PushMessagesAuthorizationRequirement requirement)
    {
        var result = context.User.Identity.IsAuthenticated;

        if (result)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
