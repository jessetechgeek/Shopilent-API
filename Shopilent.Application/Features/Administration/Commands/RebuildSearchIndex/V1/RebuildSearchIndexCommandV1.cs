using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Administration.Commands.RebuildSearchIndex.V1;

public class RebuildSearchIndexCommandV1 : ICommand<RebuildSearchIndexResponseV1>
{
    public bool InitializeIndexes { get; init; } = true;
    public bool IndexProducts { get; init; } = true;
    public bool ForceReindex { get; init; } = false;
}