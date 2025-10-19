namespace Shopilent.API.Endpoints.Payments.ProcessWebhook.V1;

public class ProcessWebhookRequestV1
{
    public string Provider { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
}