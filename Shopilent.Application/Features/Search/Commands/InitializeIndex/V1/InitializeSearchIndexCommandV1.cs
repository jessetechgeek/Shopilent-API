using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Search.Commands.InitializeIndex.V1;

public record InitializeSearchIndexCommandV1 : ICommand<InitializeSearchIndexResponseV1>;