using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.GetProductsDatatable.V1;

/// <summary>
/// Product-specific DataTable test data wrapper.
/// Uses the generic DataTableTestDataFactory for all common functionality.
/// </summary>
public static class GetProductsDatatableTestDataV1
{
    /// <summary>
    /// Standard column configuration for products datatable
    /// </summary>
    private static readonly List<DataTableColumn> _productColumns = new()
    {
        new() { Data = "name", Name = "name", Searchable = true, Orderable = true },
        new() { Data = "slug", Name = "slug", Searchable = true, Orderable = true },
        new() { Data = "sku", Name = "sku", Searchable = true, Orderable = true },
        new() { Data = "basePrice", Name = "basePrice", Searchable = false, Orderable = true },
        new() { Data = "currency", Name = "currency", Searchable = true, Orderable = true },
        new() { Data = "isActive", Name = "isActive", Searchable = false, Orderable = true },
        new() { Data = "variantsCount", Name = "variantsCount", Searchable = false, Orderable = true },
        new() { Data = "totalStockQuantity", Name = "totalStockQuantity", Searchable = false, Orderable = true },
        new() { Data = "createdAt", Name = "createdAt", Searchable = false, Orderable = true }
    };

    /// <summary>
    /// Core valid request generator for products
    /// </summary>
    public static DataTableRequest CreateValidRequest(
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchValue = null,
        bool includeColumns = true) =>
        DataTableTestDataFactory.CreateValidRequest(_productColumns, draw, start, length, searchValue, includeColumns);

    /// <summary>
    /// Pagination scenarios for products
    /// </summary>
    public static class Pagination
    {
        public static DataTableRequest CreateFirstPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateFirstPageRequest(_productColumns, pageSize);

        public static DataTableRequest CreateSecondPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateSecondPageRequest(_productColumns, pageSize);

        public static DataTableRequest CreateLargePageRequest() =>
            DataTableTestDataFactory.Pagination.CreateLargePageRequest(_productColumns);

        public static DataTableRequest CreateSmallPageRequest() =>
            DataTableTestDataFactory.Pagination.CreateSmallPageRequest(_productColumns);

        public static DataTableRequest CreateZeroLengthRequest() =>
            DataTableTestDataFactory.Pagination.CreateZeroLengthRequest(_productColumns);

        public static DataTableRequest CreateHighStartRequest() =>
            DataTableTestDataFactory.Pagination.CreateHighStartRequest(_productColumns);
    }

    /// <summary>
    /// Search scenarios for products
    /// </summary>
    public static class SearchScenarios
    {
        public static DataTableRequest CreateNameSearchRequest(string searchTerm = "laptop") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_productColumns, searchTerm);

        public static DataTableRequest CreateSlugSearchRequest(string searchTerm = "laptop") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_productColumns, searchTerm);

        public static DataTableRequest CreateSkuSearchRequest(string searchTerm = "SKU") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_productColumns, searchTerm);

        public static DataTableRequest CreateCurrencySearchRequest(string searchTerm = "USD") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_productColumns, searchTerm);

        public static DataTableRequest CreateEmptySearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateEmptySearchRequest(_productColumns);

        public static DataTableRequest CreateSpaceSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpaceSearchRequest(_productColumns);

        public static DataTableRequest CreateSpecialCharacterSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpecialCharacterSearchRequest(_productColumns);

        public static DataTableRequest CreateUnicodeSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_productColumns, "Café");

        public static DataTableRequest CreateNoResultsSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateNoResultsSearchRequest(_productColumns);
    }

    /// <summary>
    /// Sorting scenarios for products
    /// </summary>
    public static class SortingScenarios
    {
        public static DataTableRequest CreateSortByNameAscRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnAscRequest(_productColumns);

        public static DataTableRequest CreateSortByNameDescRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnDescRequest(_productColumns);

        public static DataTableRequest CreateSortBySlugRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortBySecondColumnRequest(_productColumns);

        public static DataTableRequest CreateSortBySkuRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByThirdColumnRequest(_productColumns);

        public static DataTableRequest CreateSortByPriceRequest()
        {
            var request = DataTableTestDataFactory.CreateValidRequest(_productColumns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 3, Dir = "asc" } // BasePrice column
            };
            return request;
        }

        public static DataTableRequest CreateSortByVariantsCountRequest()
        {
            var request = DataTableTestDataFactory.CreateValidRequest(_productColumns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 6, Dir = "desc" } // VariantsCount column
            };
            return request;
        }

        public static DataTableRequest CreateSortByStockRequest()
        {
            var request = DataTableTestDataFactory.CreateValidRequest(_productColumns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 7, Dir = "desc" } // TotalStockQuantity column
            };
            return request;
        }

        public static DataTableRequest CreateSortByCreatedAtRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByLastColumnRequest(_productColumns);

        public static DataTableRequest CreateMultiColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateMultiColumnSortRequest(_productColumns);

        public static DataTableRequest CreateInvalidColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateInvalidColumnSortRequest(_productColumns);
    }

    /// <summary>
    /// Validation test cases for products
    /// </summary>
    public static class ValidationTests
    {
        public static DataTableRequest CreateNegativeStartRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeStartRequest(_productColumns);

        public static DataTableRequest CreateNegativeLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeLengthRequest(_productColumns);

        public static DataTableRequest CreateNegativeDrawRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeDrawRequest(_productColumns);

        public static DataTableRequest CreateExcessiveLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateExcessiveLengthRequest(_productColumns);

        public static DataTableRequest CreateNoColumnsRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoColumnsRequest(_productColumns);

        public static DataTableRequest CreateNoOrderRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoOrderRequest(_productColumns);

        public static DataTableRequest CreateInvalidDirectionRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateInvalidDirectionRequest(_productColumns);
    }

    /// <summary>
    /// Edge case scenarios for products
    /// </summary>
    public static class EdgeCases
    {
        public static DataTableRequest CreateMaxPageSizeRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateMaxPageSizeRequest(_productColumns);

        public static DataTableRequest CreateRegexSearchRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateRegexSearchRequest(_productColumns);

        public static DataTableRequest CreateLongSearchTermRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateLongSearchTermRequest(_productColumns);

        public static DataTableRequest CreateComplexRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateComplexRequest(_productColumns);
    }

    /// <summary>
    /// Boundary test scenarios for products
    /// </summary>
    public static class BoundaryTests
    {
        public static DataTableRequest CreateMinimumValidRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateMinimumValidRequest(_productColumns);

        public static DataTableRequest CreateZeroDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateZeroDrawRequest(_productColumns);

        public static DataTableRequest CreateHighDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateHighDrawRequest(_productColumns);

        public static DataTableRequest CreateBoundaryPageRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateBoundaryPageRequest(_productColumns);
    }
}
