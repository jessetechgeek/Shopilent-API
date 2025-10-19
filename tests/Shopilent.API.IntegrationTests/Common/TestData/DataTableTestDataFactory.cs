using Bogus;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Common.TestData;

/// <summary>
/// Generic factory for creating DataTable test data across all entities.
/// Eliminates code duplication between entity-specific DataTable test classes.
/// </summary>
public static class DataTableTestDataFactory
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Core valid request generator that accepts entity-specific columns
    /// </summary>
    public static DataTableRequest CreateValidRequest(
        List<DataTableColumn> columns,
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchValue = null,
        bool includeColumns = true)
    {
        var request = new DataTableRequest
        {
            Draw = draw,
            Start = start,
            Length = length,
            Search = new DataTableSearch
            {
                Value = searchValue ?? string.Empty,
                Regex = false
            }
        };

        if (includeColumns)
        {
            request.Columns = columns;
            request.Order = CreateStandardOrder();
        }

        return request;
    }

    /// <summary>
    /// Creates standard order (first column ascending)
    /// </summary>
    private static List<DataTableOrder> CreateStandardOrder()
    {
        return new List<DataTableOrder>
        {
            new() { Column = 0, Dir = "asc" } // Order by first column ascending by default
        };
    }

    /// <summary>
    /// Pagination test scenarios - works for any entity
    /// </summary>
    public static class Pagination
    {
        public static DataTableRequest CreateFirstPageRequest(List<DataTableColumn> columns, int pageSize = 10) =>
            CreateValidRequest(columns, start: 0, length: pageSize);

        public static DataTableRequest CreateSecondPageRequest(List<DataTableColumn> columns, int pageSize = 10) =>
            CreateValidRequest(columns, start: pageSize, length: pageSize);

        public static DataTableRequest CreateLargePageRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: 0, length: 100);

        public static DataTableRequest CreateSmallPageRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: 0, length: 1);

        public static DataTableRequest CreateZeroLengthRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: 0, length: 0);

        public static DataTableRequest CreateHighStartRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: 9999, length: 10);
    }

    /// <summary>
    /// Search test scenarios - works for any entity
    /// </summary>
    public static class SearchScenarios
    {
        public static DataTableRequest CreateGenericSearchRequest(List<DataTableColumn> columns, string searchTerm = "test") =>
            CreateValidRequest(columns, searchValue: searchTerm);

        public static DataTableRequest CreateEmptySearchRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, searchValue: "");

        public static DataTableRequest CreateSpaceSearchRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, searchValue: " ");

        public static DataTableRequest CreateSpecialCharacterSearchRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, searchValue: "-_");

        public static DataTableRequest CreateUnicodeSearchRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, searchValue: "Größe");

        public static DataTableRequest CreateNoResultsSearchRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, searchValue: "nonexistentitem12345");
    }

    /// <summary>
    /// Sorting test scenarios - works for any entity
    /// </summary>
    public static class SortingScenarios
    {
        public static DataTableRequest CreateSortByFirstColumnAscRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 0, Dir = "asc" } // First column
            };
            return request;
        }

        public static DataTableRequest CreateSortByFirstColumnDescRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 0, Dir = "desc" } // First column
            };
            return request;
        }

        public static DataTableRequest CreateSortBySecondColumnRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 1, Dir = "asc" } // Second column
            };
            return request;
        }

        public static DataTableRequest CreateSortByThirdColumnRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 2, Dir = "asc" } // Third column
            };
            return request;
        }

        public static DataTableRequest CreateSortByLastColumnRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = columns.Count - 1, Dir = "desc" } // Last column (usually CreatedAt)
            };
            return request;
        }

        public static DataTableRequest CreateMultiColumnSortRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = Math.Min(2, columns.Count - 1), Dir = "asc" },  // Third column or last available
                new() { Column = 0, Dir = "asc" }   // Then first column
            };
            return request;
        }

        public static DataTableRequest CreateInvalidColumnSortRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 99, Dir = "asc" } // Invalid column index
            };
            return request;
        }
    }

    /// <summary>
    /// Validation test cases - works for any entity
    /// </summary>
    public static class ValidationTests
    {
        public static DataTableRequest CreateNegativeStartRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: -1, length: 10);

        public static DataTableRequest CreateNegativeLengthRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: 0, length: -1);

        public static DataTableRequest CreateNegativeDrawRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, draw: -1, start: 0, length: 10);

        public static DataTableRequest CreateExcessiveLengthRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: 0, length: 10000);

        public static DataTableRequest CreateNoColumnsRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Columns.Clear();
            return request;
        }

        public static DataTableRequest CreateNoOrderRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order.Clear();
            return request;
        }

        public static DataTableRequest CreateInvalidDirectionRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns);
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 0, Dir = "invalid" }
            };
            return request;
        }
    }

    /// <summary>
    /// Edge case test scenarios - works for any entity
    /// </summary>
    public static class EdgeCases
    {
        public static DataTableRequest CreateMaxPageSizeRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: 0, length: 1000);

        public static DataTableRequest CreateRegexSearchRequest(List<DataTableColumn> columns)
        {
            var request = CreateValidRequest(columns, searchValue: "test.*");
            request.Search.Regex = true;
            return request;
        }

        public static DataTableRequest CreateLongSearchTermRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, searchValue: new string('a', 1000));

        public static DataTableRequest CreateComplexRequest(List<DataTableColumn> columns)
        {
            var request = new DataTableRequest
            {
                Draw = 5,
                Start = 20,
                Length = 25,
                Search = new DataTableSearch
                {
                    Value = "test",
                    Regex = false
                },
                Columns = columns.Take(Math.Min(4, columns.Count)).Select((col, index) => new DataTableColumn
                {
                    Data = col.Data,
                    Name = col.Name,
                    Searchable = col.Searchable,
                    Orderable = col.Orderable,
                    Search = new DataTableSearch { Value = index == 0 ? "search" : "" }
                }).ToList(),
                Order = new List<DataTableOrder>
                {
                    new() { Column = Math.Min(1, columns.Count - 1), Dir = "desc" },
                    new() { Column = Math.Min(2, columns.Count - 1), Dir = "asc" }
                }
            };
            return request;
        }
    }

    /// <summary>
    /// Boundary test scenarios - works for any entity
    /// </summary>
    public static class BoundaryTests
    {
        public static DataTableRequest CreateMinimumValidRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, draw: 1, start: 0, length: 1);

        public static DataTableRequest CreateZeroDrawRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, draw: 0, start: 0, length: 10);

        public static DataTableRequest CreateHighDrawRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, draw: int.MaxValue, start: 0, length: 10);

        public static DataTableRequest CreateBoundaryPageRequest(List<DataTableColumn> columns) =>
            CreateValidRequest(columns, start: int.MaxValue - 1000, length: 10);
    }
}