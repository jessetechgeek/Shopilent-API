using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.GetUsersDatatable.V1;

/// <summary>
/// User-specific DataTable test data wrapper.
/// Uses the generic DataTableTestDataFactory for all common functionality.
/// </summary>
public static class GetUsersDatatableTestDataV1
{
    /// <summary>
    /// Standard column configuration for users datatable
    /// </summary>
    private static readonly List<DataTableColumn> _userColumns = new()
    {
        new() { Data = "email", Name = "email", Searchable = true, Orderable = true },
        new() { Data = "fullName", Name = "fullName", Searchable = true, Orderable = true },
        new() { Data = "phone", Name = "phone", Searchable = true, Orderable = false },
        new() { Data = "roleName", Name = "roleName", Searchable = true, Orderable = true },
        new() { Data = "isActive", Name = "isActive", Searchable = false, Orderable = true },
        new() { Data = "createdAt", Name = "createdAt", Searchable = false, Orderable = true }
    };

    /// <summary>
    /// Core valid request generator for users
    /// </summary>
    public static DataTableRequest CreateValidRequest(
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchValue = null,
        bool includeColumns = true) =>
        DataTableTestDataFactory.CreateValidRequest(_userColumns, draw, start, length, searchValue, includeColumns);

    /// <summary>
    /// Pagination scenarios for users
    /// </summary>
    public static class Pagination
    {
        public static DataTableRequest CreateFirstPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateFirstPageRequest(_userColumns, pageSize);

        public static DataTableRequest CreateSecondPageRequest(int pageSize = 10) =>
            DataTableTestDataFactory.Pagination.CreateSecondPageRequest(_userColumns, pageSize);

        public static DataTableRequest CreateLargePageRequest() =>
            DataTableTestDataFactory.Pagination.CreateLargePageRequest(_userColumns);

        public static DataTableRequest CreateSmallPageRequest() =>
            DataTableTestDataFactory.Pagination.CreateSmallPageRequest(_userColumns);

        public static DataTableRequest CreateZeroLengthRequest() =>
            DataTableTestDataFactory.Pagination.CreateZeroLengthRequest(_userColumns);

        public static DataTableRequest CreateHighStartRequest() =>
            DataTableTestDataFactory.Pagination.CreateHighStartRequest(_userColumns);
    }

    /// <summary>
    /// Search scenarios for users
    /// </summary>
    public static class SearchScenarios
    {
        public static DataTableRequest CreateEmailSearchRequest(string searchTerm = "john") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_userColumns, searchTerm);

        public static DataTableRequest CreateFullNameSearchRequest(string searchTerm = "John") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_userColumns, searchTerm);

        public static DataTableRequest CreateRoleSearchRequest(string searchTerm = "admin") =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_userColumns, searchTerm);

        public static DataTableRequest CreateEmptySearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateEmptySearchRequest(_userColumns);

        public static DataTableRequest CreateSpaceSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpaceSearchRequest(_userColumns);

        public static DataTableRequest CreateSpecialCharacterSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateSpecialCharacterSearchRequest(_userColumns);

        public static DataTableRequest CreateUnicodeSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateGenericSearchRequest(_userColumns, "Müller");

        public static DataTableRequest CreateNoResultsSearchRequest() =>
            DataTableTestDataFactory.SearchScenarios.CreateNoResultsSearchRequest(_userColumns);
    }

    /// <summary>
    /// Sorting scenarios for users
    /// </summary>
    public static class SortingScenarios
    {
        public static DataTableRequest CreateSortByEmailAscRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnAscRequest(_userColumns);

        public static DataTableRequest CreateSortByEmailDescRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByFirstColumnDescRequest(_userColumns);

        public static DataTableRequest CreateSortByFullNameRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortBySecondColumnRequest(_userColumns);

        public static DataTableRequest CreateSortByRoleRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByThirdColumnRequest(_userColumns);

        public static DataTableRequest CreateSortByCreatedAtRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateSortByLastColumnRequest(_userColumns);

        public static DataTableRequest CreateMultiColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateMultiColumnSortRequest(_userColumns);

        public static DataTableRequest CreateInvalidColumnSortRequest() =>
            DataTableTestDataFactory.SortingScenarios.CreateInvalidColumnSortRequest(_userColumns);
    }

    /// <summary>
    /// Validation test cases for users
    /// </summary>
    public static class ValidationTests
    {
        public static DataTableRequest CreateNegativeStartRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeStartRequest(_userColumns);

        public static DataTableRequest CreateNegativeLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeLengthRequest(_userColumns);

        public static DataTableRequest CreateNegativeDrawRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNegativeDrawRequest(_userColumns);

        public static DataTableRequest CreateExcessiveLengthRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateExcessiveLengthRequest(_userColumns);

        public static DataTableRequest CreateNoColumnsRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoColumnsRequest(_userColumns);

        public static DataTableRequest CreateNoOrderRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateNoOrderRequest(_userColumns);

        public static DataTableRequest CreateInvalidDirectionRequest() =>
            DataTableTestDataFactory.ValidationTests.CreateInvalidDirectionRequest(_userColumns);
    }

    /// <summary>
    /// Edge case scenarios for users
    /// </summary>
    public static class EdgeCases
    {
        public static DataTableRequest CreateMaxPageSizeRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateMaxPageSizeRequest(_userColumns);

        public static DataTableRequest CreateRegexSearchRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateRegexSearchRequest(_userColumns);

        public static DataTableRequest CreateLongSearchTermRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateLongSearchTermRequest(_userColumns);

        public static DataTableRequest CreateComplexRequest() =>
            DataTableTestDataFactory.EdgeCases.CreateComplexRequest(_userColumns);
    }

    /// <summary>
    /// Boundary test scenarios for users
    /// </summary>
    public static class BoundaryTests
    {
        public static DataTableRequest CreateMinimumValidRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateMinimumValidRequest(_userColumns);

        public static DataTableRequest CreateZeroDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateZeroDrawRequest(_userColumns);

        public static DataTableRequest CreateHighDrawRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateHighDrawRequest(_userColumns);

        public static DataTableRequest CreateBoundaryPageRequest() =>
            DataTableTestDataFactory.BoundaryTests.CreateBoundaryPageRequest(_userColumns);
    }
}