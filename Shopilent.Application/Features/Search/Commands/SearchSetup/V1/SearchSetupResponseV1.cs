namespace Shopilent.Application.Features.Search.Commands.SearchSetup.V1;

public class SearchSetupResponseV1
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = "";
    public bool IndexesInitialized { get; set; }
    public int ProductsIndexed { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
}