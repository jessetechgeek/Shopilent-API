using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Catalog.Errors;

public static class AttributeErrors
{
    public static Error NameRequired => Error.Validation(
        code: "Attribute.NameRequired",
        message: "Attribute name cannot be empty.");

    public static Error DisplayNameRequired => Error.Validation(
        code: "Attribute.DisplayNameRequired",
        message: "Attribute display name cannot be empty.");

    public static Error InvalidAttributeType => Error.Validation(
        code: "Attribute.InvalidType",
        message: "Invalid attribute type specified.");

    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Attribute.NotFound",
        message: $"Attribute with ID {id} was not found.");

    public static Error DuplicateName(string name) => Error.Conflict(
        code: "Attribute.DuplicateName",
        message: $"An attribute with name '{name}' already exists.");

    public static Error InvalidConfigurationFormat => Error.Validation(
        code: "Attribute.InvalidConfigurationFormat",
        message: "The attribute configuration format is invalid.");

    public static Error NotVariantAttribute(string name) =>
        Error.Validation(
            code: "Attribute.NotVariantAttribute",
            message: $"Attribute '{name}' is not a variant attribute.");
}