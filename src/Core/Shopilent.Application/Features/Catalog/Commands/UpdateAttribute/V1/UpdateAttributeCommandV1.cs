using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateAttribute.V1;

public sealed record UpdateAttributeCommandV1 : ICommand<UpdateAttributeResponseV1>
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string DisplayName { get; init; }
    public bool Filterable { get; init; }
    public bool Searchable { get; init; }
    public bool IsVariant { get; init; }
    public Dictionary<string, object> Configuration { get; init; }
}