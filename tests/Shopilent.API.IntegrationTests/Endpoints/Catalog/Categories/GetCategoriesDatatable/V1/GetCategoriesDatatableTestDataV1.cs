using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetCategoriesDatatable.V1;

/// <summary>
/// Category-specific DataTable test data wrapper.
/// Uses the generic DataTableTestDataFactory for all common functionality.
/// </summary>
public static class GetCategoriesDatatableTestDataV1
{
    /// <summary>
    /// Standard column configuration for categories datatable
    /// </summary>
    private static readonly List<DataTableColumn> _categoryColumns = new()
    {
        new() { Data = "name", Name = "name", Searchable = true, Orderable = true },
        new() { Data = "slug", Name = "slug", Searchable = true, Orderable = true },
        new() { Data = "description", Name = "description", Searchable = true, Orderable = true },
        new() { Data = "parentName", Name = "parentName", Searchable = true, Orderable = true },
        new() { Data = "level", Name = "level", Searchable = false, Orderable = true },
        new() { Data = "isActive", Name = "isActive", Searchable = false, Orderable = true },
        new() { Data = "productCount", Name = "productCount", Searchable = false, Orderable = true },
        new() { Data = "createdAt", Name = "createdAt", Searchable = false, Orderable = true }
    };

    /// <summary>
    /// Core valid request generator for categories
    /// </summary>
    public static DataTableRequest CreateValidRequest(
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchValue = null,
        bool includeColumns = true) =>
        DataTableTestDataFactory.CreateValidRequest(_categoryColumns, draw, start, length, searchValue, includeColumns);

    /// <summary>
    /// Pagination scenarios for categories
    /// </summary>
    public static class Pagination
    {
        public static DataTableRequest CreateFirstPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateFirstPageRequest(_categoryColumns, pageSize);

        public static DataTableRequest CreateSecondPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateSecondPageRequest(_categoryColumns, pageSize);

        public static DataTableRequest CreateLargePageRequest() =>
            DataTableTestDataFactory.Pagination.CreateLargePageRequest(_categoryColumns);

        public static DataTableRequest CreateSmallPageRequest() =>
            DataTableTestDataFactory.Pagination.CreateSmallPageRequest(_categoryColumns);

        public static DataTableRequest CreateZeroLengthRequest() =>
            DataTableTestDataFactory.Pagination.CreateZeroLengthRequest(_categoryColumns);

        public static DataTableRequest CreateHighStartRequest() =>
            DataTableTestDataFactory.Pagination.CreateHighStartRequest(_categoryColumns);
    }

    /// <summary>
    /// Search scenarios for categories
    /// </summary>
    public static class SearchScenarios
    {
        public static DataTableRequest CreateNameSearchRequest(string searchTerm = "electronics") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_categoryColumns, searchTerm);

        public static DataTableRequest CreateSlugSearchRequest(string searchTerm = "electronics") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_categoryColumns, searchTerm);

        public static DataTableRequest CreateDescriptionSearchRequest(string searchTerm = "devices") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_categoryColumns, searchTerm);

        public static DataTableRequest CreateParentSearchRequest(string searchTerm = "parent") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_categoryColumns, searchTerm);

        public static DataTableRequest CreateEmptySearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateEmptySearchRequest(_categoryColumns);

        public static DataTableRequest CreateSpaceSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpaceSearchRequest(_categoryColumns);

        public static DataTableRequest CreateSpecialCharacterSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpecialCharacterSearchRequest(_categoryColumns);

        public static DataTableRequest CreateUnicodeSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_categoryColumns, "Möbel");

        public static DataTableRequest CreateNoResultsSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateNoResultsSearchRequest(_categoryColumns);
    }

    /// <summary>
    /// Sorting scenarios for categories
    /// </summary>
    public static class SortingScenarios
    {
        public static DataTableRequest CreateSortByNameAscRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnAscRequest(_categoryColumns);

        public static DataTableRequest CreateSortByNameDescRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnDescRequest(_categoryColumns);

        public static DataTableRequest CreateSortBySlugRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortBySecondColumnRequest(_categoryColumns);

        public static DataTableRequest CreateSortByDescriptionRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByThirdColumnRequest(_categoryColumns);

        public static DataTableRequest CreateSortByLevelRequest()
        {
            var request = DataTableTestDataFactory.CreateValidRequest(_categoryColumns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 4, Dir = "asc" } // Level column
            };
            return request;
        }

        public static DataTableRequest CreateSortByCreatedAtRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByLastColumnRequest(_categoryColumns);

        public static DataTableRequest CreateMultiColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateMultiColumnSortRequest(_categoryColumns);

        public static DataTableRequest CreateInvalidColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateInvalidColumnSortRequest(_categoryColumns);
    }

    /// <summary>
    /// Validation test cases for categories
    /// </summary>
    public static class ValidationTests
    {
        public static DataTableRequest CreateNegativeStartRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeStartRequest(_categoryColumns);

        public static DataTableRequest CreateNegativeLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeLengthRequest(_categoryColumns);

        public static DataTableRequest CreateNegativeDrawRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeDrawRequest(_categoryColumns);

        public static DataTableRequest CreateExcessiveLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateExcessiveLengthRequest(_categoryColumns);

        public static DataTableRequest CreateNoColumnsRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoColumnsRequest(_categoryColumns);

        public static DataTableRequest CreateNoOrderRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoOrderRequest(_categoryColumns);

        public static DataTableRequest CreateInvalidDirectionRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateInvalidDirectionRequest(_categoryColumns);
    }

    /// <summary>
    /// Edge case scenarios for categories
    /// </summary>
    public static class EdgeCases
    {
        public static DataTableRequest CreateMaxPageSizeRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateMaxPageSizeRequest(_categoryColumns);

        public static DataTableRequest CreateRegexSearchRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateRegexSearchRequest(_categoryColumns);

        public static DataTableRequest CreateLongSearchTermRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateLongSearchTermRequest(_categoryColumns);

        public static DataTableRequest CreateComplexRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateComplexRequest(_categoryColumns);
    }

    /// <summary>
    /// Boundary test scenarios for categories
    /// </summary>
    public static class BoundaryTests
    {
        public static DataTableRequest CreateMinimumValidRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateMinimumValidRequest(_categoryColumns);

        public static DataTableRequest CreateZeroDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateZeroDrawRequest(_categoryColumns);

        public static DataTableRequest CreateHighDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateHighDrawRequest(_categoryColumns);

        public static DataTableRequest CreateBoundaryPageRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateBoundaryPageRequest(_categoryColumns);
    }
}