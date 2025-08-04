namespace Shopilent.API.Endpoints.Administration.RebuildSearchIndex.V1;

public class RebuildSearchIndexRequestV1
{
    public bool InitializeIndexes { get; init; } = true;
    public bool IndexProducts { get; init; } = true;
    public bool ForceReindex { get; init; } = false;
}