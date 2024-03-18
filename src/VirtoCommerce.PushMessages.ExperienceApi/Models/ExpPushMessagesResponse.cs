using System.Collections.Generic;

namespace VirtoCommerce.PushMessages.ExperienceApi.Models
{
    public class ExpPushMessagesResponse
    {
        public int UnreadCount { get; set; }

        public IList<ExpPushMessage> Items { get; set; } = [];
    }
}
