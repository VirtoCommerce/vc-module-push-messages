namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class MarkPushMessageUnreadCommand : PushMessagesCommand
    {
        public string MessageId { get; set; }
    }
}
