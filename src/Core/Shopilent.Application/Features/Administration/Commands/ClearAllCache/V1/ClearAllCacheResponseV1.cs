namespace Shopilent.Application.Features.Administration.Commands.ClearAllCache.V1;

public class ClearAllCacheResponseV1
{
    public string Message { get; set; } = string.Empty;
    public DateTime ClearedAt { get; set; }
    public int KeysCleared { get; set; }
}