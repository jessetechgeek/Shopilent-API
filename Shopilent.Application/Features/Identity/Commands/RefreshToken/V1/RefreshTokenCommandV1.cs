using MediatR;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Features.Identity.Commands.Login.V1;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;

namespace Shopilent.Application.Features.Identity.Commands.RefreshToken.V1;

public sealed record RefreshTokenCommandV1 : ICommand<RefreshTokenResponseV1>
{
    public string RefreshToken { get; init; }
    public string IpAddress { get; init; }
    public string UserAgent { get; init; }
}