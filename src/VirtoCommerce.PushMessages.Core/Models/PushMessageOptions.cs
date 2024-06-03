namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageOptions
{
    public bool UseFirebaseCloudMessaging { get; set; }
    public FcmSenderOptions FcmSenderOptions { get; set; }
    public FcmReceiverOptions FcmReceiverOptions { get; set; }
}
