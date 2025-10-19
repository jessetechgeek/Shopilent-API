namespace Shopilent.Domain.Common.Models;

public class DataTableResult<T>
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public IReadOnlyList<T> Data { get; set; }
    public string Error { get; set; }

    public DataTableResult(int draw, int recordsTotal, int recordsFiltered, IReadOnlyList<T> data)
    {
        Draw = draw;
        RecordsTotal = recordsTotal;
        RecordsFiltered = recordsFiltered;
        Data = data;
    }

    public DataTableResult(int draw, string error)
    {
        Draw = draw;
        Error = error;
        Data = new List<T>();
    }
}