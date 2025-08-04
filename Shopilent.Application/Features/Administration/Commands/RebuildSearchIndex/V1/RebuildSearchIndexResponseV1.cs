namespace Shopilent.Application.Features.Administration.Commands.RebuildSearchIndex.V1;

public class RebuildSearchIndexResponseV1
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = "";
    public bool IndexesInitialized { get; set; }
    public int ProductsIndexed { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
}