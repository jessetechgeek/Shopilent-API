using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Features.Catalog.Queries.GetAttributesDatatable.V1;

public sealed record GetAttributesDatatableQueryV1 : IQuery<DataTableResult<AttributeDatatableDto>>
{
    public DataTableRequest Request { get; init; }
}