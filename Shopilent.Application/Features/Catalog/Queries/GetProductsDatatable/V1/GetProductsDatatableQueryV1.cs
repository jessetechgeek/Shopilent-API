using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Features.Catalog.Queries.GetProductsDatatable.V1;

public sealed record GetProductsDatatableQueryV1 : IQuery<DataTableResult<ProductDatatableDto>>
{
    public DataTableRequest Request { get; init; }
}