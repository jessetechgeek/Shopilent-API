using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Search.Commands.ReindexProducts.V1;

public record ReindexProductsCommandV1 : ICommand<ReindexProductsResponseV1>;