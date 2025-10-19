using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.GetAttributesDatatable.V1;

/// <summary>
/// Attribute-specific DataTable test data wrapper.
/// Uses the generic DataTableTestDataFactory for all common functionality.
/// </summary>
public static class GetAttributesDatatableTestDataV1
{
    /// <summary>
    /// Standard column configuration for attributes datatable
    /// </summary>
    private static readonly List<DataTableColumn> _attributeColumns = new()
    {
        new() { Data = "name", Name = "name", Searchable = true, Orderable = true },
        new() { Data = "displayName", Name = "displayName", Searchable = true, Orderable = true },
        new() { Data = "type", Name = "type", Searchable = true, Orderable = true },
        new() { Data = "filterable", Name = "filterable", Searchable = false, Orderable = true },
        new() { Data = "searchable", Name = "searchable", Searchable = false, Orderable = true },
        new() { Data = "isVariant", Name = "isVariant", Searchable = false, Orderable = true },
        new() { Data = "createdAt", Name = "createdAt", Searchable = false, Orderable = true }
    };

    /// <summary>
    /// Core valid request generator for attributes
    /// </summary>
    public static DataTableRequest CreateValidRequest(
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchValue = null,
        bool includeColumns = true) =>
        DataTableTestDataFactory.CreateValidRequest(_attributeColumns, draw, start, length, searchValue, includeColumns);

    /// <summary>
    /// Pagination scenarios for attributes
    /// </summary>
    public static class Pagination
    {
        public static DataTableRequest CreateFirstPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateFirstPageRequest(_attributeColumns, pageSize);

        public static DataTableRequest CreateSecondPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateSecondPageRequest(_attributeColumns, pageSize);

        public static DataTableRequest CreateLargePageRequest() =>
            DataTableTestDataFactory.Pagination.CreateLargePageRequest(_attributeColumns);

        public static DataTableRequest CreateSmallPageRequest() =>
            DataTableTestDataFactory.Pagination.CreateSmallPageRequest(_attributeColumns);

        public static DataTableRequest CreateZeroLengthRequest() =>
            DataTableTestDataFactory.Pagination.CreateZeroLengthRequest(_attributeColumns);

        public static DataTableRequest CreateHighStartRequest() =>
            DataTableTestDataFactory.Pagination.CreateHighStartRequest(_attributeColumns);
    }

    /// <summary>
    /// Search scenarios for attributes
    /// </summary>
    public static class SearchScenarios
    {
        public static DataTableRequest CreateNameSearchRequest(string searchTerm = "color") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_attributeColumns, searchTerm);

        public static DataTableRequest CreateDisplayNameSearchRequest(string searchTerm = "Color") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_attributeColumns, searchTerm);

        public static DataTableRequest CreateTypeSearchRequest(string searchTerm = "text") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_attributeColumns, searchTerm);

        public static DataTableRequest CreateEmptySearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateEmptySearchRequest(_attributeColumns);

        public static DataTableRequest CreateSpaceSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpaceSearchRequest(_attributeColumns);

        public static DataTableRequest CreateSpecialCharacterSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpecialCharacterSearchRequest(_attributeColumns);

        public static DataTableRequest CreateUnicodeSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_attributeColumns, "Größe");

        public static DataTableRequest CreateNoResultsSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateNoResultsSearchRequest(_attributeColumns);
    }

    /// <summary>
    /// Sorting scenarios for attributes
    /// </summary>
    public static class SortingScenarios
    {
        public static DataTableRequest CreateSortByNameAscRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnAscRequest(_attributeColumns);

        public static DataTableRequest CreateSortByNameDescRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnDescRequest(_attributeColumns);

        public static DataTableRequest CreateSortByDisplayNameRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortBySecondColumnRequest(_attributeColumns);

        public static DataTableRequest CreateSortByTypeRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByThirdColumnRequest(_attributeColumns);

        public static DataTableRequest CreateSortByCreatedAtRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByLastColumnRequest(_attributeColumns);

        public static DataTableRequest CreateMultiColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateMultiColumnSortRequest(_attributeColumns);

        public static DataTableRequest CreateInvalidColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateInvalidColumnSortRequest(_attributeColumns);
    }

    /// <summary>
    /// Validation test cases for attributes
    /// </summary>
    public static class ValidationTests
    {
        public static DataTableRequest CreateNegativeStartRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeStartRequest(_attributeColumns);

        public static DataTableRequest CreateNegativeLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeLengthRequest(_attributeColumns);

        public static DataTableRequest CreateNegativeDrawRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeDrawRequest(_attributeColumns);

        public static DataTableRequest CreateExcessiveLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateExcessiveLengthRequest(_attributeColumns);

        public static DataTableRequest CreateNoColumnsRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoColumnsRequest(_attributeColumns);

        public static DataTableRequest CreateNoOrderRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoOrderRequest(_attributeColumns);

        public static DataTableRequest CreateInvalidDirectionRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateInvalidDirectionRequest(_attributeColumns);
    }

    /// <summary>
    /// Edge case scenarios for attributes
    /// </summary>
    public static class EdgeCases
    {
        public static DataTableRequest CreateMaxPageSizeRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateMaxPageSizeRequest(_attributeColumns);

        public static DataTableRequest CreateRegexSearchRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateRegexSearchRequest(_attributeColumns);

        public static DataTableRequest CreateLongSearchTermRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateLongSearchTermRequest(_attributeColumns);

        public static DataTableRequest CreateComplexRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateComplexRequest(_attributeColumns);
    }

    /// <summary>
    /// Boundary test scenarios for attributes
    /// </summary>
    public static class BoundaryTests
    {
        public static DataTableRequest CreateMinimumValidRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateMinimumValidRequest(_attributeColumns);

        public static DataTableRequest CreateZeroDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateZeroDrawRequest(_attributeColumns);

        public static DataTableRequest CreateHighDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateHighDrawRequest(_attributeColumns);

        public static DataTableRequest CreateBoundaryPageRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateBoundaryPageRequest(_attributeColumns);
    }
}