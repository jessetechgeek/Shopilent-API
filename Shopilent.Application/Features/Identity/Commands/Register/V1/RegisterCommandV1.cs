using MediatR;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Features.Identity.Commands.Login.V1;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;

namespace Shopilent.Application.Features.Identity.Commands.Register.V1;

public sealed record RegisterCommandV1 : ICommand<RegisterResponseV1>
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Phone { get; init; }
    public string IpAddress { get; init; }
    public string UserAgent { get; init; }
}