using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.Application.UnitTests.Testing.Builders;

public class AttributeBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Attribute";
    private string _displayName = "Test Attribute";
    private AttributeType _type = AttributeType.Text;
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public AttributeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
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
    
    public AttributeBuilder IsInactive()
    {
        _isActive = false;
        return this;
    }
    
    public AttributeBuilder CreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Domain.Catalog.Attribute Build()
    {
        var attributeResult = Domain.Catalog.Attribute.Create(_name, _displayName, _type);
        if (attributeResult.IsFailure)
            throw new InvalidOperationException($"Failed to create attribute: {attributeResult.Error.Message}");
            
        var attribute = attributeResult.Value;
        
        // Use reflection to set private properties
        SetPrivatePropertyValue(attribute, "Id", _id);
        SetPrivatePropertyValue(attribute, "CreatedAt", _createdAt);
        SetPrivatePropertyValue(attribute, "UpdatedAt", _updatedAt);
        
        // Note: IsActive property might be set via reflection if needed
        if (!_isActive)
        {
            SetPrivatePropertyValue(attribute, "IsActive", false);
        }
        
        return attribute;
    }
    
    private static void SetPrivatePropertyValue<T>(object obj, string propertyName, T value)
    {
        var propertyInfo = obj.GetType().GetProperty(propertyName);
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(obj, value, null);
        }
        else
        {
            var fieldInfo = obj.GetType().GetField(propertyName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
                throw new InvalidOperationException($"Property or field {propertyName} not found on type {obj.GetType().Name}");
            }
        }
    }
}