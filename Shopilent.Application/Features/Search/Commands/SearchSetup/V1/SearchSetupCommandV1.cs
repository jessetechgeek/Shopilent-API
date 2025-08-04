using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Search.Commands.SearchSetup.V1;

public class SearchSetupCommandV1 : ICommand<SearchSetupResponseV1>
{
    public bool InitializeIndexes { get; init; } = true;
    public bool IndexProducts { get; init; } = true;
    public bool ForceReindex { get; init; } = false;
}