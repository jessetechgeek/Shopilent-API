using Shopilent.API.Common.Models;
using Shopilent.Domain.Common.Results;

namespace Shopilent.API.Common.Services;

public interface IFilterEncodingService
{
    Result<ProductFilters> DecodeFilters(string base64EncodedFilters);
    string EncodeFilters(ProductFilters filters);
}