using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.Application.Features.Catalog.Commands.CreateAttribute.V1;

public sealed record CreateAttributeCommandV1 : ICommand<CreateAttributeResponseV1>
{
    public string Name { get; init; }
    public string DisplayName { get; init; }
    public AttributeType Type { get; init; }
    public bool Filterable { get; init; }
    public bool Searchable { get; init; }
    public bool IsVariant { get; init; }
    public Dictionary<string, object> Configuration { get; init; }
}