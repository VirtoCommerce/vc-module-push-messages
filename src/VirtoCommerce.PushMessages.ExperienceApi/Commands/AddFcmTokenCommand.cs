using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands;

public class AddFcmTokenCommand : ICommand<bool>
{
    public string Token { get; set; }

    public string UserId { get; set; }
}
