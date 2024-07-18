using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands;

public class DeleteFcmTokenCommand : ICommand<bool>
{
    public string Token { get; set; }

    public string UserId { get; set; }
}
