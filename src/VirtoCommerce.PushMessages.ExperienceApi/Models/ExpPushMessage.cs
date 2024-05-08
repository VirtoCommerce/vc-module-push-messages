using System;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Models
{
    public class ExpPushMessage
    {
        public string Id { get; set; }

        public string ShortMessage { get; set; }

        public DateTime CreatedDate { get; set; }

        public string UserId { get; set; }

        public bool IsRead { get; set; }

        public bool IsHidden { get; set; }

        public static ExpPushMessage Create(PushMessage message, PushMessageRecipient recipient)
        {
            var expPushMessage = AbstractTypeFactory<ExpPushMessage>.TryCreateInstance();

            expPushMessage.Id = message.Id;
            expPushMessage.ShortMessage = message.ShortMessage;
            expPushMessage.CreatedDate = recipient.CreatedDate;
            expPushMessage.UserId = recipient.UserId;
            expPushMessage.IsRead = recipient.IsRead;
            expPushMessage.IsHidden = recipient.IsHidden;

            return expPushMessage;
        }
    }
}
