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

        RuleFor(x => x.ExpectedCheckInInterval)
            .Must(v => v is null || v > TimeSpan.Zero)
            .WithMessage("Expected check-in interval must be positive.");
    }
}