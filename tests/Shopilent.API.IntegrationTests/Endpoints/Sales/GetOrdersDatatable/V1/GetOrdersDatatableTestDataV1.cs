using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Sales.GetOrdersDatatable.V1;

/// <summary>
/// Order-specific DataTable test data wrapper.
/// Uses the generic DataTableTestDataFactory for all common functionality.
/// </summary>
public static class GetOrdersDatatableTestDataV1
{
    /// <summary>
    /// Standard column configuration for orders datatable
    /// </summary>
    private static readonly List<DataTableColumn> _orderColumns = new()
    {
        new() { Data = "id", Name = "id", Searchable = false, Orderable = true },
        new() { Data = "userEmail", Name = "userEmail", Searchable = true, Orderable = true },
        new() { Data = "userFullName", Name = "userFullName", Searchable = true, Orderable = true },
        new() { Data = "total", Name = "total", Searchable = false, Orderable = true },
        new() { Data = "status", Name = "status", Searchable = true, Orderable = true },
        new() { Data = "paymentStatus", Name = "paymentStatus", Searchable = true, Orderable = true },
        new() { Data = "shippingMethod", Name = "shippingMethod", Searchable = true, Orderable = true },
        new() { Data = "trackingNumber", Name = "trackingNumber", Searchable = true, Orderable = false },
        new() { Data = "createdAt", Name = "createdAt", Searchable = false, Orderable = true }
    };

    /// <summary>
    /// Core valid request generator for orders
    /// </summary>
    public static DataTableRequest CreateValidRequest(
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchValue = null,
        bool includeColumns = true) =>
        DataTableTestDataFactory.CreateValidRequest(_orderColumns, draw, start, length, searchValue, includeColumns);

    /// <summary>
    /// Pagination scenarios for orders
    /// </summary>
    public static class Pagination
    {
        public static DataTableRequest CreateFirstPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateFirstPageRequest(_orderColumns, pageSize);

        public static DataTableRequest CreateSecondPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateSecondPageRequest(_orderColumns, pageSize);

        public static DataTableRequest CreateLargePageRequest() =>
            DataTableTestDataFactory.Pagination.CreateLargePageRequest(_orderColumns);

        public static DataTableRequest CreateSmallPageRequest() =>
            DataTableTestDataFactory.Pagination.CreateSmallPageRequest(_orderColumns);

        public static DataTableRequest CreateZeroLengthRequest() =>
            DataTableTestDataFactory.Pagination.CreateZeroLengthRequest(_orderColumns);

        public static DataTableRequest CreateHighStartRequest() =>
            DataTableTestDataFactory.Pagination.CreateHighStartRequest(_orderColumns);
    }

    /// <summary>
    /// Search scenarios for orders
    /// </summary>
    public static class SearchScenarios
    {
        public static DataTableRequest CreateUserEmailSearchRequest(string searchTerm = "customer") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_orderColumns, searchTerm);

        public static DataTableRequest CreateUserFullNameSearchRequest(string searchTerm = "Customer") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_orderColumns, searchTerm);

        public static DataTableRequest CreateStatusSearchRequest(string searchTerm = "pending") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_orderColumns, searchTerm);

        public static DataTableRequest CreatePaymentStatusSearchRequest(string searchTerm = "paid") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_orderColumns, searchTerm);

        public static DataTableRequest CreateShippingMethodSearchRequest(string searchTerm = "express") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_orderColumns, searchTerm);

        public static DataTableRequest CreateTrackingNumberSearchRequest(string searchTerm = "TRACK") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_orderColumns, searchTerm);

        public static DataTableRequest CreateEmptySearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateEmptySearchRequest(_orderColumns);

        public static DataTableRequest CreateSpaceSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpaceSearchRequest(_orderColumns);

        public static DataTableRequest CreateSpecialCharacterSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpecialCharacterSearchRequest(_orderColumns);

        public static DataTableRequest CreateUnicodeSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateUnicodeSearchRequest(_orderColumns);

        public static DataTableRequest CreateNoResultsSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateNoResultsSearchRequest(_orderColumns);
    }

    /// <summary>
    /// Sorting scenarios for orders
    /// </summary>
    public static class SortingScenarios
    {
        public static DataTableRequest CreateSortByIdAscRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnAscRequest(_orderColumns);

        public static DataTableRequest CreateSortByIdDescRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnDescRequest(_orderColumns);

        public static DataTableRequest CreateSortByUserEmailRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortBySecondColumnRequest(_orderColumns);

        public static DataTableRequest CreateSortByTotalRequest()
        {
            var request = DataTableTestDataFactory.CreateValidRequest(_orderColumns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 3, Dir = "asc" } // Total column
            };
            return request;
        }

        public static DataTableRequest CreateSortByStatusRequest()
        {
            var request = DataTableTestDataFactory.CreateValidRequest(_orderColumns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 4, Dir = "asc" } // Status column
            };
            return request;
        }

        public static DataTableRequest CreateSortByCreatedAtRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByLastColumnRequest(_orderColumns);

        public static DataTableRequest CreateMultiColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateMultiColumnSortRequest(_orderColumns);

        public static DataTableRequest CreateInvalidColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateInvalidColumnSortRequest(_orderColumns);
    }

    /// <summary>
    /// Validation test cases for orders
    /// </summary>
    public static class ValidationTests
    {
        public static DataTableRequest CreateNegativeStartRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeStartRequest(_orderColumns);

        public static DataTableRequest CreateNegativeLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeLengthRequest(_orderColumns);

        public static DataTableRequest CreateNegativeDrawRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeDrawRequest(_orderColumns);

        public static DataTableRequest CreateExcessiveLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateExcessiveLengthRequest(_orderColumns);

        public static DataTableRequest CreateNoColumnsRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoColumnsRequest(_orderColumns);

        public static DataTableRequest CreateNoOrderRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoOrderRequest(_orderColumns);

        public static DataTableRequest CreateInvalidDirectionRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateInvalidDirectionRequest(_orderColumns);
    }

    /// <summary>
    /// Edge case scenarios for orders
    /// </summary>
    public static class EdgeCases
    {
        public static DataTableRequest CreateMaxPageSizeRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateMaxPageSizeRequest(_orderColumns);

        public static DataTableRequest CreateRegexSearchRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateRegexSearchRequest(_orderColumns);

        public static DataTableRequest CreateLongSearchTermRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateLongSearchTermRequest(_orderColumns);

        public static DataTableRequest CreateComplexRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateComplexRequest(_orderColumns);
    }

    /// <summary>
    /// Boundary test scenarios for orders
    /// </summary>
    public static class BoundaryTests
    {
        public static DataTableRequest CreateMinimumValidRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateMinimumValidRequest(_orderColumns);

        public static DataTableRequest CreateZeroDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateZeroDrawRequest(_orderColumns);

        public static DataTableRequest CreateHighDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateHighDrawRequest(_orderColumns);

        public static DataTableRequest CreateBoundaryPageRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateBoundaryPageRequest(_orderColumns);
    }
}
