using Bogus;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.GetUsersDatatable.V1;

public static class GetUsersDatatableTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static DataTableRequest CreateValidRequest(
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
            request.Columns = CreateStandardColumns();
            request.Order = CreateStandardOrder();
        }

        return request;
    }

    // Standard column configuration for users datatable
    private static List<DataTableColumn> CreateStandardColumns()
    {
        return new List<DataTableColumn>
        {
            new() { Data = "email", Name = "email", Searchable = true, Orderable = true },
            new() { Data = "fullName", Name = "fullName", Searchable = true, Orderable = true },
            new() { Data = "phone", Name = "phone", Searchable = true, Orderable = false },
            new() { Data = "roleName", Name = "roleName", Searchable = true, Orderable = true },
            new() { Data = "isActive", Name = "isActive", Searchable = false, Orderable = true },
            new() { Data = "createdAt", Name = "createdAt", Searchable = false, Orderable = true }
        };
    }

    private static List<DataTableOrder> CreateStandardOrder()
    {
        return new List<DataTableOrder>
        {
            new() { Column = 0, Dir = "asc" } // Order by email ascending by default
        };
    }

    // Pagination scenarios
    public static class Pagination
    {
        public static DataTableRequest CreateFirstPageRequest(int pageSize = 10) => 
            CreateValidRequest(start: 0, length: pageSize);

        public static DataTableRequest CreateSecondPageRequest(int pageSize = 10) => 
            CreateValidRequest(start: pageSize, length: pageSize);

        public static DataTableRequest CreateLargePageRequest() => 
            CreateValidRequest(start: 0, length: 100);

        public static DataTableRequest CreateSmallPageRequest() => 
            CreateValidRequest(start: 0, length: 1);

        public static DataTableRequest CreateZeroLengthRequest() => 
            CreateValidRequest(start: 0, length: 0);

        public static DataTableRequest CreateHighStartRequest() => 
            CreateValidRequest(start: 9999, length: 10);
    }

    // Search scenarios
    public static class SearchScenarios
    {
        public static DataTableRequest CreateEmailSearchRequest(string searchTerm = "admin") =>
            CreateValidRequest(searchValue: searchTerm);

        public static DataTableRequest CreateNameSearchRequest(string searchTerm = "John") =>
            CreateValidRequest(searchValue: searchTerm);

        public static DataTableRequest CreateRoleSearchRequest(string searchTerm = "Admin") =>
            CreateValidRequest(searchValue: searchTerm);

        public static DataTableRequest CreateEmptySearchRequest() =>
            CreateValidRequest(searchValue: "");

        public static DataTableRequest CreateSpaceSearchRequest() =>
            CreateValidRequest(searchValue: " ");

        public static DataTableRequest CreateSpecialCharacterSearchRequest() =>
            CreateValidRequest(searchValue: "@#$%");

        public static DataTableRequest CreateUnicodeSearchRequest() =>
            CreateValidRequest(searchValue: "Müller");

        public static DataTableRequest CreateNoResultsSearchRequest() =>
            CreateValidRequest(searchValue: "nonexistentuser12345");
    }

    // Sorting scenarios
    public static class SortingScenarios
    {
        public static DataTableRequest CreateSortByEmailAscRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 0, Dir = "asc" } // Email column
            };
            return request;
        }

        public static DataTableRequest CreateSortByEmailDescRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 0, Dir = "desc" } // Email column
            };
            return request;
        }

        public static DataTableRequest CreateSortByFullNameRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 1, Dir = "asc" } // FullName column
            };
            return request;
        }

        public static DataTableRequest CreateSortByRoleRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 3, Dir = "asc" } // Role column
            };
            return request;
        }

        public static DataTableRequest CreateSortByCreatedAtRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 5, Dir = "desc" } // CreatedAt column
            };
            return request;
        }

        public static DataTableRequest CreateMultiColumnSortRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 3, Dir = "asc" },  // Role first
                new() { Column = 1, Dir = "asc" }   // Then FullName
            };
            return request;
        }

        public static DataTableRequest CreateInvalidColumnSortRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 99, Dir = "asc" } // Invalid column index
            };
            return request;
        }
    }

    // Validation test cases
    public static class ValidationTests
    {
        public static DataTableRequest CreateNegativeStartRequest() =>
            CreateValidRequest(start: -1, length: 10);

        public static DataTableRequest CreateNegativeLengthRequest() =>
            CreateValidRequest(start: 0, length: -1);

        public static DataTableRequest CreateNegativeDrawRequest() =>
            CreateValidRequest(draw: -1, start: 0, length: 10);

        public static DataTableRequest CreateExcessiveLengthRequest() =>
            CreateValidRequest(start: 0, length: 10000);

        public static DataTableRequest CreateNoColumnsRequest()
        {
            var request = CreateValidRequest();
            request.Columns.Clear();
            return request;
        }

        public static DataTableRequest CreateNoOrderRequest()
        {
            var request = CreateValidRequest();
            request.Order.Clear();
            return request;
        }

        public static DataTableRequest CreateInvalidDirectionRequest()
        {
            var request = CreateValidRequest();
            request.Order = new List<DataTableOrder>
            {
                new() { Column = 0, Dir = "invalid" }
            };
            return request;
        }
    }

    // Edge cases
    public static class EdgeCases
    {
        public static DataTableRequest CreateMaxPageSizeRequest() =>
            CreateValidRequest(start: 0, length: 1000);

        public static DataTableRequest CreateRegexSearchRequest()
        {
            var request = CreateValidRequest(searchValue: "admin.*");
            request.Search.Regex = true;
            return request;
        }

        public static DataTableRequest CreateLongSearchTermRequest() =>
            CreateValidRequest(searchValue: new string('a', 1000));

        public static DataTableRequest CreateComplexRequest()
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
                Columns = new List<DataTableColumn>
                {
                    new() { Data = "email", Name = "email", Searchable = true, Orderable = true, Search = new DataTableSearch { Value = "admin" } },
                    new() { Data = "fullName", Name = "fullName", Searchable = true, Orderable = true, Search = new DataTableSearch { Value = "" } },
                    new() { Data = "phone", Name = "phone", Searchable = true, Orderable = false, Search = new DataTableSearch { Value = "" } },
                    new() { Data = "roleName", Name = "roleName", Searchable = true, Orderable = true, Search = new DataTableSearch { Value = "Manager" } }
                },
                Order = new List<DataTableOrder>
                {
                    new() { Column = 1, Dir = "desc" },
                    new() { Column = 3, Dir = "asc" }
                }
            };
            return request;
        }
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static DataTableRequest CreateMinimumValidRequest() =>
            CreateValidRequest(draw: 1, start: 0, length: 1);

        public static DataTableRequest CreateZeroDrawRequest() =>
            CreateValidRequest(draw: 0, start: 0, length: 10);

        public static DataTableRequest CreateHighDrawRequest() =>
            CreateValidRequest(draw: int.MaxValue, start: 0, length: 10);

        public static DataTableRequest CreateBoundaryPageRequest() =>
            CreateValidRequest(start: int.MaxValue - 1000, length: 10);
    }
}