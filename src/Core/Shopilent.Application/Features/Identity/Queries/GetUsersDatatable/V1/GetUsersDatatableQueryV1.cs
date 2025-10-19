using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Features.Identity.Queries.GetUsersDatatable.V1;

public sealed record GetUsersDatatableQueryV1 : IQuery<DataTableResult<UserDatatableDto>>
{
    public DataTableRequest Request { get; init; }
}