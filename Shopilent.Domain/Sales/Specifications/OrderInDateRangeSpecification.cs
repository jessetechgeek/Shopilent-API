using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Sales.Specifications;

public class OrderInDateRangeSpecification : Specification<Order>
    {
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        public OrderInDateRangeSpecification(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate;
            _endDate = endDate;
        }

        public override bool IsSatisfiedBy(Order order)
        {
            return order.CreatedAt >= _startDate && order.CreatedAt <= _endDate;
        }
    }