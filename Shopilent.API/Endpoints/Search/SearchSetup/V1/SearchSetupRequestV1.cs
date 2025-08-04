namespace Shopilent.API.Endpoints.Search.SearchSetup.V1;

public class SearchSetupRequestV1
{
    public bool InitializeIndexes { get; init; } = true;
    public bool IndexProducts { get; init; } = true;
    public bool ForceReindex { get; init; } = false;
}