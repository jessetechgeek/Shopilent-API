namespace Shopilent.API.IntegrationTests.Common;

public class TestDataTableResult<T>
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<T> Data { get; set; } = new();
    public string Error { get; set; } = string.Empty;
}