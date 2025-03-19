using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog;

public class Attribute : AggregateRoot
{
    private Attribute()
    {
        // Required by EF Core
    }

    private Attribute(string name, string displayName, AttributeType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        Name = name;
        DisplayName = displayName;
        Type = type;
        Configuration = new Dictionary<string, object>();
    }

    public static Attribute Create(string name, string displayName, AttributeType type)
    {
        var attribute = new Attribute(name, displayName, type);
        attribute.AddDomainEvent(new AttributeCreatedEvent(attribute.Id));
        return attribute;
    }

    public static Attribute CreateFilterable(string name, string displayName, AttributeType type)
    {
        var attribute = Create(name, displayName, type);
        attribute.Filterable = true;
        return attribute;
    }

    public static Attribute CreateSearchable(string name, string displayName, AttributeType type)
    {
        var attribute = Create(name, displayName, type);
        attribute.Searchable = true;
        return attribute;
    }

    public static Attribute CreateVariant(string name, string displayName, AttributeType type)
    {
        var attribute = Create(name, displayName, type);
        attribute.IsVariant = true;
        return attribute;
    }

    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public AttributeType Type { get; private set; }
    public Dictionary<string, object> Configuration { get; private set; } = new();
    public bool Filterable { get; private set; }
    public bool Searchable { get; private set; }
    public bool IsVariant { get; private set; }

    public void SetFilterable(bool filterable)
    {
        Filterable = filterable;
    }

    public void SetSearchable(bool searchable)
    {
        Searchable = searchable;
    }

    public void SetIsVariant(bool isVariant)
    {
        IsVariant = isVariant;
    }

    public void UpdateConfiguration(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(key));

        Configuration[key] = value;
    }
}