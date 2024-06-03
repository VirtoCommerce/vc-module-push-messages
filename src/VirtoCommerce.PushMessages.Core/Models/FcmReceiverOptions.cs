namespace VirtoCommerce.PushMessages.Core.Models;

public class FcmReceiverOptions
{
    public string ApiKey { get; set; }
    public string AuthDomain { get; set; }
    public string ProjectId { get; set; }
    public string StorageBucket { get; set; }
    public string MessagingSenderId { get; set; }
    public string AppId { get; set; }
    public string VapidKey { get; set; }
}
