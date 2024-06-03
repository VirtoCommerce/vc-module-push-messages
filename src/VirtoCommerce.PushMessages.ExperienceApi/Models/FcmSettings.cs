namespace VirtoCommerce.PushMessages.ExperienceApi.Models;

public class FcmSettings
{
    public string ApiKey { get; set; }
    public string AuthDomain { get; set; }
    public string ProjectId { get; set; }
    public string StorageBucket { get; set; }
    public string MessagingSenderId { get; set; }
    public string AppId { get; set; }
    public string VapidKey { get; set; }
}
