namespace Shopilent.Domain.Common.Models;

public class DataTableOrder
{
    public int Column { get; set; }
    public string Dir { get; set; }

    public bool IsDescending => Dir?.ToLower() == "desc";
}