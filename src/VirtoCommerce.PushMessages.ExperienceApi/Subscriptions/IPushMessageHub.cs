using System;
using System.Threading.Tasks;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Subscriptions
{
    public interface IPushMessageHub
    {
        Task<ExpPushMessage> AddMessageAsync(ExpPushMessage message);
        Task<IObservable<ExpPushMessage>> MessagesAsync();
    }
}
