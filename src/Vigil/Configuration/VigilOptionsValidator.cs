using Microsoft.Extensions.Options;

namespace Vigil.Configuration;

internal sealed class VigilOptionsValidator : IValidateOptions<VigilOptions>
{
    public ValidateOptionsResult Validate(string? name, VigilOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AdminKey))
        {
            return ValidateOptionsResult.Fail(
                "Configuration value 'AdminKey' must be set to a non-empty value. " +
                "Startup aborted because admin-protected endpoints cannot be secured.");
        }

        return ValidateOptionsResult.Success;
    }
}
