using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public abstract class PushMessagesCommand : ICommand<bool>
    {
        public string UserId { get; set; }
    }
}
