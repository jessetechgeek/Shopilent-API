using MediatR;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}