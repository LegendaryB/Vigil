using System.Text;

namespace Vigil.Domain.Errors;

internal abstract class DomainErrorCatalog
{
    protected abstract string Prefix { get; }
    
    internal string PropertyRequired => Prefix + "property_required";
    
    internal string PropertyValueMustBeUnique => Prefix + "property_value_must_be_unique";
    
    internal string EntityNotFound => Prefix + "entity_not_found";

    internal static string PropertyRequiredMessage(string propertyName) =>
        UseMessageTemplate(
            PropertyRequiredMessageTemplate,
            propertyName);
    
    internal static string PropertyValueMustBeUniqueMessage(string propertyName) =>
        UseMessageTemplate(
            PropertyValueMustBeUniqueMessageTemplate,
            propertyName);

    internal static string EntityNotFoundMessage(string entityName, Guid id) =>
        UseMessageTemplate(
            EntityNotFoundMessageTemplate,
            entityName,
            id);
    
    private static readonly CompositeFormat PropertyRequiredMessageTemplate =
        CompositeFormat.Parse("The property '{0}' is required.");
    
    private static readonly CompositeFormat PropertyValueMustBeUniqueMessageTemplate =
        CompositeFormat.Parse("The value of property '{0}' must be unique.");
    
    private static readonly CompositeFormat EntityNotFoundMessageTemplate =
        CompositeFormat.Parse("The {0} was not found (id={1}).");
    
    protected static string UseMessageTemplate(CompositeFormat format, params object?[] args) =>
        string.Format(
            null,
            format,
            args);
}