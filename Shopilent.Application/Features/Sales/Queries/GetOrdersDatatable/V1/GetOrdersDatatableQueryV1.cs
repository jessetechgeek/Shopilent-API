using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Features.Sales.Queries.GetOrdersDatatable.V1;

public sealed record GetOrdersDatatableQueryV1 : IQuery<DataTableResult<OrderDatatableDto>>
{
    public DataTableRequest Request { get; init; }
}