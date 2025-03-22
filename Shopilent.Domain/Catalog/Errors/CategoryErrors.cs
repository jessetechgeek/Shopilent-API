using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Catalog.Errors;

public static class CategoryErrors
{
    public static Error NameRequired => Error.Validation(
        code: "Category.NameRequired",
        message: "Category name cannot be empty.");

    public static Error SlugRequired => Error.Validation(
        code: "Category.SlugRequired",
        message: "Category slug cannot be empty.");

    public static Error DuplicateSlug(string slug) => Error.Conflict(
        code: "Category.DuplicateSlug",
        message: $"A category with slug '{slug}' already exists.");

    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Category.NotFound",
        message: $"Category with ID {id} was not found.");
    
    public static Error CircularReference => Error.Validation(
        code: "Category.CircularReference",
        message: "Cannot set parent category as it would create a circular reference.");
        
    public static Error InvalidCategoryStatus => Error.Validation(
        code: "Category.InvalidStatus",
        message: "Cannot associate product with an inactive category.");
        
    public static Error MaxDepthReached => Error.Validation(
        code: "Category.MaxDepthReached",
        message: "Cannot add more levels to the category hierarchy. Maximum depth reached.");
        
    public static Error CannotDeleteWithProducts => Error.Validation(
        code: "Category.CannotDeleteWithProducts",
        message: "Cannot delete a category that has associated products.");
        
    public static Error CannotDeleteWithChildren => Error.Validation(
        code: "Category.CannotDeleteWithChildren",
        message: "Cannot delete a category that has child categories.");
}