using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Payments;

namespace Shopilent.Application.Features.Payments.Commands.ProcessWebhook.V1;

public class ProcessWebhookCommandV1 : ICommand<WebhookResult>
{
    public string Provider { get; set; } = string.Empty;
    public string WebhookPayload { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
}