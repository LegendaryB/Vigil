using FluentValidation;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Errors;

namespace Vigil.Features.ClientKeys;

internal class UpdateClientKeyRequestValidator : AbstractValidator<UpdateClientKeyFeature.Request>
{
    public UpdateClientKeyRequestValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty()
            .WithMessage(DomainErrorCatalog.PropertyRequiredMessage(nameof(ClientKey.ClientName)))
            .WithErrorCode(ErrorCatalog.ClientKey.PropertyRequired);
    }
}
