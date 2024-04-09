using System;

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
    }
}
