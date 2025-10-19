using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Features.Catalog.Queries.GetCategoriesDatatable.V1;

public sealed record GetCategoriesDatatableQueryV1 : IQuery<DataTableResult<CategoryDatatableDto>>
{
    public DataTableRequest Request { get; init; }
}