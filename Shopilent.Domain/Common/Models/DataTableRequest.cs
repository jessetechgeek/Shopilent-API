namespace Shopilent.Domain.Common.Models;

public class DataTableRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public List<DataTableColumn> Columns { get; set; } = new List<DataTableColumn>();
    public List<DataTableOrder> Order { get; set; } = new List<DataTableOrder>();
    public DataTableSearch Search { get; set; } = new DataTableSearch();
    public int PageNumber => Length > 0 ? Start / Length + 1 : 1;
    public int PageSize => Length;
}