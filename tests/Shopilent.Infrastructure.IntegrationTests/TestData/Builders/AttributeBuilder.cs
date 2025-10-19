using Bogus;
using Shopilent.Domain.Catalog.Enums;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

public class AttributeBuilder
{
    private string _name;
    private string _displayName;
    private AttributeType _type;
    private bool _filterable;
    private bool _searchable;
    private bool _isVariant;
    private readonly Dictionary<string, object> _configuration;

    public AttributeBuilder()
    {
        var faker = new Faker();
        _name = $"{faker.Commerce.ProductAdjective().ToLower().Replace(" ", "_")}_{DateTime.Now.Ticks}_{Guid.NewGuid().ToString("N")[..8]}";
        _displayName = faker.Commerce.ProductAdjective();
        _type = faker.PickRandom<AttributeType>();
        _filterable = false;
        _searchable = false;
        _isVariant = false;
        _configuration = new Dictionary<string, object>();
    }

    public AttributeBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public AttributeBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public AttributeBuilder WithType(AttributeType type)
    {
        _type = type;
        return this;
    }

    public AttributeBuilder AsFilterable()
    {
        _filterable = true;
        return this;
    }

    public AttributeBuilder AsSearchable()
    {
        _searchable = true;
        return this;
    }

    public AttributeBuilder AsVariant()
    {
        _isVariant = true;
        return this;
    }

    public AttributeBuilder WithConfiguration(string key, object value)
    {
        _configuration[key] = value;
        return this;
    }

    public Attribute Build()
    {
        var attribute = Attribute.Create(_name, _displayName, _type).Value;

        if (_filterable)
        {
            attribute.SetFilterable(true);
        }

        if (_searchable)
        {
            attribute.SetSearchable(true);
        }

        if (_isVariant)
        {
            attribute.SetIsVariant(true);
        }

        foreach (var config in _configuration)
        {
            attribute.UpdateConfiguration(config.Key, config.Value);
        }

        return attribute;
    }

    public static AttributeBuilder Random()
    {
        return new AttributeBuilder();
    }

    public static AttributeBuilder FilterableAttribute(string name = null)
    {
        var builder = new AttributeBuilder();
        if (!string.IsNullOrEmpty(name))
        {
            builder.WithName(name).WithDisplayName(name.Replace("_", " "));
        }
        return builder.AsFilterable();
    }

    public static AttributeBuilder SearchableAttribute(string name = null)
    {
        var builder = new AttributeBuilder();
        if (!string.IsNullOrEmpty(name))
        {
            builder.WithName(name).WithDisplayName(name.Replace("_", " "));
        }
        return builder.AsSearchable();
    }

    public static AttributeBuilder VariantAttribute(string name = null, AttributeType type = AttributeType.Text)
    {
        var builder = new AttributeBuilder();
        if (!string.IsNullOrEmpty(name))
        {
            builder.WithName(name).WithDisplayName(name.Replace("_", " "));
        }
        return builder.WithType(type).AsVariant();
    }

    public static List<Attribute> CreateMany(int count)
    {
        var attributes = new List<Attribute>();
        var usedNames = new HashSet<string>();
        var faker = new Faker();

        for (int i = 0; i < count; i++)
        {
            // Generate a unique name for each attribute
            var baseName = $"test_attribute_{DateTime.Now.Ticks}_{i}";
            var uniqueName = baseName;
            int suffix = 1;

            while (usedNames.Contains(uniqueName))
            {
                uniqueName = $"{baseName}_{suffix}";
                suffix++;
            }

            usedNames.Add(uniqueName);

            var attribute = Random().WithName(uniqueName);
            attributes.Add(attribute.Build());
        }

        return attributes;
    }
}
