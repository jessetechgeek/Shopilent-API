namespace Shopilent.Application.Features.Search.Commands.InitializeIndex.V1;

public class InitializeSearchIndexResponseV1
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime InitializedAt { get; set; }
}