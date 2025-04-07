using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog;

public class Attribute : AggregateRoot
{
    private Attribute()
    {
        // Required by EF Core
    }

    private Attribute(string name, string displayName, AttributeType type)
    {
        Name = name;
        DisplayName = displayName;
        Type = type;
        Configuration = new Dictionary<string, object>();
    }

    public static Result<Attribute> Create(string name, string displayName, AttributeType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Attribute>(AttributeErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<Attribute>(AttributeErrors.DisplayNameRequired);

        var attribute = new Attribute(name, displayName, type);
        attribute.AddDomainEvent(new AttributeCreatedEvent(attribute.Id));
        return Result.Success(attribute);
    }

    public static Result<Attribute> CreateFilterable(string name, string displayName, AttributeType type)
    {
        var attributeResult = Create(name, displayName, type);
        if (attributeResult.IsFailure)
            return attributeResult;

        var attribute = attributeResult.Value;
        attribute.Filterable = true;
        return Result.Success(attribute);
    }

    public static Result<Attribute> CreateSearchable(string name, string displayName, AttributeType type)
    {
        var attributeResult = Create(name, displayName, type);
        if (attributeResult.IsFailure)
            return attributeResult;

        var attribute = attributeResult.Value;
        attribute.Searchable = true;
        return Result.Success(attribute);
    }

    public static Result<Attribute> CreateVariant(string name, string displayName, AttributeType type)
    {
        var attributeResult = Create(name, displayName, type);
        if (attributeResult.IsFailure)
            return attributeResult;

        var attribute = attributeResult.Value;
        attribute.IsVariant = true;
        return Result.Success(attribute);
    }

    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public AttributeType Type { get; private set; }
    public Dictionary<string, object> Configuration { get; private set; } = new();
    public bool Filterable { get; private set; }
    public bool Searchable { get; private set; }
    public bool IsVariant { get; private set; }

    public Result SetFilterable(bool filterable)
    {
        Filterable = filterable;
        return Result.Success();
    }

    public Result SetSearchable(bool searchable)
    {
        Searchable = searchable;
        return Result.Success();
    }

    public Result SetIsVariant(bool isVariant)
    {
        IsVariant = isVariant;
        return Result.Success();
    }

    public Result UpdateConfiguration(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);

        Configuration[key] = value;
        return Result.Success();
    }


    public Result Update(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure(AttributeErrors.DisplayNameRequired);

        DisplayName = displayName;
        return Result.Success();
    }
}