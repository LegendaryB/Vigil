using FluentValidation;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Errors;

namespace Vigil.Features.ClientKeys;

internal class CreateClientKeyRequestValidator : AbstractValidator<CreateClientKeyFeature.Request>
{
    public CreateClientKeyRequestValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty()
            .WithMessage(DomainErrorCatalog.PropertyRequiredMessage(nameof(ClientKey.ClientName)))
            .WithErrorCode(ErrorCatalog.ClientKey.PropertyRequired);
    }
}