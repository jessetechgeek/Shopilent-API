using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Identity.DTOs;
using Shopilent.Domain.Payments.DTOs;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.DTOs;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Repositories.Read;
using Shopilent.Domain.Shipping.DTOs;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Sales.Read;

public class OrderReadRepository : AggregateReadRepositoryBase<Order, OrderDto>, IOrderReadRepository
{
    public OrderReadRepository(IDapperConnectionFactory connectionFactory, ILogger<OrderReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                o.id AS Id,
                o.user_id AS UserId,
                o.billing_address_id AS BillingAddressId,
                o.shipping_address_id AS ShippingAddressId,
                o.payment_method_id AS PaymentMethodId,
                o.subtotal AS Subtotal,
                o.tax AS Tax,
                o.shipping_cost AS ShippingCost,
                o.total AS Total,
                o.currency AS Currency,
                o.status AS Status,
                o.payment_status AS PaymentStatus,
                o.shipping_method AS ShippingMethod,
                o.refunded_amount AS RefundedAmount,
                o.refunded_at AS RefundedAt,
                o.refund_reason AS RefundReason,
                o.metadata AS Metadata,
                o.created_at AS CreatedAt,
                o.updated_at AS UpdatedAt
            FROM orders o
            WHERE o.id = @Id";

        var order = await Connection.QueryFirstOrDefaultAsync<OrderDto>(sql, new { Id = id });

        if (order != null)
        {
            // Extract tracking number from metadata if available
            if (order.Metadata != null && order.Metadata.TryGetValue("trackingNumber", out var trackingNumber))
            {
                order.TrackingNumber = trackingNumber.ToString();
            }

            // Load shipping address
            if (order.ShippingAddressId.HasValue)
            {
                const string shippingAddressSql = @"
                    SELECT 
                        id AS Id,
                        user_id AS UserId,
                        address_line1 AS AddressLine1,
                        address_line2 AS AddressLine2,
                        city AS City,
                        state AS State,
                        postal_code AS PostalCode,
                        country AS Country,
                        phone AS Phone,
                        is_default AS IsDefault,
                        address_type AS AddressType,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM addresses
                    WHERE id = @AddressId";

                order.ShippingAddress = await Connection.QueryFirstOrDefaultAsync<AddressDto>(
                    shippingAddressSql, new { AddressId = order.ShippingAddressId.Value });
            }

            // Load billing address
            if (order.BillingAddressId.HasValue)
            {
                const string billingAddressSql = @"
                    SELECT 
                        id AS Id,
                        user_id AS UserId,
                        address_line1 AS AddressLine1,
                        address_line2 AS AddressLine2,
                        city AS City,
                        state AS State,
                        postal_code AS PostalCode,
                        country AS Country,
                        phone AS Phone,
                        is_default AS IsDefault,
                        address_type AS AddressType,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM addresses
                    WHERE id = @AddressId";

                order.BillingAddress = await Connection.QueryFirstOrDefaultAsync<AddressDto>(
                    billingAddressSql, new { AddressId = order.BillingAddressId.Value });
            }
        }

        return order;
    }

    public async Task<OrderDetailDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get basic order info (excluding metadata to avoid casting issues)
        const string orderSql = @"
        SELECT 
            o.id AS Id,
            o.user_id AS UserId,
            o.billing_address_id AS BillingAddressId,
            o.shipping_address_id AS ShippingAddressId,
            o.payment_method_id AS PaymentMethodId,
            o.subtotal AS Subtotal,
            o.tax AS Tax,
            o.shipping_cost AS ShippingCost,
            o.total AS Total,
            o.currency AS Currency,
            o.status AS Status,
            o.payment_status AS PaymentStatus,
            o.shipping_method AS ShippingMethod,
            o.refunded_amount AS RefundedAmount,
            o.refunded_at AS RefundedAt,
            o.refund_reason AS RefundReason,
            o.created_by AS CreatedBy,
            o.modified_by AS ModifiedBy,
            o.last_modified AS LastModified,
            o.created_at AS CreatedAt,
            o.updated_at AS UpdatedAt
        FROM orders o
        WHERE o.id = @Id";

        var orderDetail = await Connection.QueryFirstOrDefaultAsync<OrderDetailDto>(orderSql, new { Id = id });

        if (orderDetail != null)
        {
            // Load metadata separately to avoid casting issues
            const string metadataSql = "SELECT metadata FROM orders WHERE id = @Id";
            var metadataJson = await Connection.QueryFirstOrDefaultAsync<string>(metadataSql, new { Id = id });

            if (!string.IsNullOrEmpty(metadataJson))
            {
                try
                {
                    var metadata =
                        JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
                    orderDetail.Metadata = metadata;

                    // Extract tracking number from metadata if available
                    if (metadata != null && metadata.TryGetValue("trackingNumber", out var trackingNumber))
                    {
                        orderDetail.TrackingNumber = trackingNumber.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to parse metadata for order {OrderId}", id);
                }
            }

            // Load shipping address
            if (orderDetail.ShippingAddressId.HasValue)
            {
                const string shippingAddressSql = @"
                SELECT 
                    id AS Id,
                    user_id AS UserId,
                    address_line1 AS AddressLine1,
                    address_line2 AS AddressLine2,
                    city AS City,
                    state AS State,
                    postal_code AS PostalCode,
                    country AS Country,
                    phone AS Phone,
                    is_default AS IsDefault,
                    address_type AS AddressType,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM addresses
                WHERE id = @AddressId";

                orderDetail.ShippingAddress = await Connection.QueryFirstOrDefaultAsync<AddressDto>(
                    shippingAddressSql, new { AddressId = orderDetail.ShippingAddressId.Value });
            }

            // Load billing address
            if (orderDetail.BillingAddressId.HasValue)
            {
                const string billingAddressSql = @"
                SELECT 
                    id AS Id,
                    user_id AS UserId,
                    address_line1 AS AddressLine1,
                    address_line2 AS AddressLine2,
                    city AS City,
                    state AS State,
                    postal_code AS PostalCode,
                    country AS Country,
                    phone AS Phone,
                    is_default AS IsDefault,
                    address_type AS AddressType,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM addresses
                WHERE id = @AddressId";

                orderDetail.BillingAddress = await Connection.QueryFirstOrDefaultAsync<AddressDto>(
                    billingAddressSql, new { AddressId = orderDetail.BillingAddressId.Value });
            }

            // Load user information
            if (orderDetail.UserId.HasValue)
            {
                const string userSql = @"
                SELECT 
                    id AS Id,
                    email AS Email,
                    first_name AS FirstName,
                    last_name AS LastName,
                    middle_name AS MiddleName,
                    phone AS Phone,
                    role AS Role,
                    is_active AS IsActive,
                    last_login AS LastLogin,
                    email_verified AS EmailVerified,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM users
                WHERE id = @UserId";

                orderDetail.User = await Connection.QueryFirstOrDefaultAsync<UserDto>(
                    userSql, new { UserId = orderDetail.UserId.Value });
            }

            // Load payment method
            if (orderDetail.PaymentMethodId.HasValue)
            {
                const string paymentMethodSql = @"
                SELECT 
                    id AS Id,
                    user_id AS UserId,
                    type AS Type,
                    provider AS Provider,
                    display_name AS DisplayName,
                    card_brand AS CardBrand,
                    last_four_digits AS LastFourDigits,
                    expiry_date AS ExpiryDate,
                    is_default AS IsDefault,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM payment_methods
                WHERE id = @PaymentMethodId";

                orderDetail.PaymentMethod = await Connection.QueryFirstOrDefaultAsync<PaymentMethodDto>(
                    paymentMethodSql, new { PaymentMethodId = orderDetail.PaymentMethodId.Value });
            }

            // Load payments
            const string paymentsSql = @"
            SELECT 
                id AS Id,
                order_id AS OrderId,
                user_id AS UserId,
                amount AS Amount,
                currency AS Currency,
                method AS MethodType,
                provider AS Provider,
                status AS Status,
                external_reference AS ExternalReference,
                transaction_id AS TransactionId,
                payment_method_id AS PaymentMethodId,
                processed_at AS ProcessedAt,
                error_message AS ErrorMessage,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM payments
            WHERE order_id = @OrderId";

            orderDetail.Payments = (await Connection.QueryAsync<PaymentDto>(
                paymentsSql, new { OrderId = id })).ToList();

            // Load order items (excluding product_data to avoid casting issues)
            const string itemsSql = @"
            SELECT 
                id AS Id,
                order_id AS OrderId,
                product_id AS ProductId,
                variant_id AS VariantId,
                quantity AS Quantity,
                unit_price AS UnitPrice,
                total_price AS TotalPrice,
                currency AS Currency,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM order_items
            WHERE order_id = @OrderId";

            var items = await Connection.QueryAsync<OrderItemDto>(itemsSql, new { OrderId = id });
            var itemsList = items.ToList();

            // Load product_data separately for each item
            foreach (var item in itemsList)
            {
                const string productDataSql = "SELECT product_data FROM order_items WHERE id = @ItemId";
                var productDataJson =
                    await Connection.QueryFirstOrDefaultAsync<string>(productDataSql, new { ItemId = item.Id });

                if (!string.IsNullOrEmpty(productDataJson))
                {
                    try
                    {
                        var productData =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(productDataJson);
                        item.ProductData = productData;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Failed to parse product_data for order item {ItemId}", item.Id);
                    }
                }
            }

            orderDetail.Items = itemsList;
        }

        return orderDetail;
    }

    public override async Task<IReadOnlyList<OrderDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                billing_address_id AS BillingAddressId,
                shipping_address_id AS ShippingAddressId,
                payment_method_id AS PaymentMethodId,
                subtotal AS Subtotal,
                tax AS Tax,
                shipping_cost AS ShippingCost,
                total AS Total,
                currency AS Currency,
                status AS Status,
                payment_status AS PaymentStatus,
                shipping_method AS ShippingMethod,
                refunded_amount AS RefundedAmount,
                refunded_at AS RefundedAt,
                refund_reason AS RefundReason,
                metadata AS Metadata,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM orders
            ORDER BY created_at DESC";

        var orderDtos = await Connection.QueryAsync<OrderDto>(sql);
        return orderDtos.ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                user_id AS UserId,
                billing_address_id AS BillingAddressId,
                shipping_address_id AS ShippingAddressId,
                payment_method_id AS PaymentMethodId,
                subtotal AS Subtotal,
                tax AS Tax,
                shipping_cost AS ShippingCost,
                total AS Total,
                currency AS Currency,
                status AS Status,
                payment_status AS PaymentStatus,
                shipping_method AS ShippingMethod,
                refunded_amount AS RefundedAmount,
                refunded_at AS RefundedAt,
                refund_reason AS RefundReason,
                metadata AS Metadata,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM orders
            WHERE user_id = @UserId
            ORDER BY created_at DESC";

        var orderDtos = await Connection.QueryAsync<OrderDto>(sql, new { UserId = userId });
        var orderList = orderDtos.ToList();

        // Extract tracking number from metadata if available for each order
        foreach (var order in orderList)
        {
            if (order.Metadata != null && order.Metadata.TryGetValue("trackingNumber", out var trackingNumber))
            {
                order.TrackingNumber = trackingNumber.ToString();
            }
        }

        return orderList;
    }

    public async Task<IReadOnlyList<OrderDto>> GetByStatusAsync(OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                billing_address_id AS BillingAddressId,
                shipping_address_id AS ShippingAddressId,
                payment_method_id AS PaymentMethodId,
                subtotal AS Subtotal,
                tax AS Tax,
                shipping_cost AS ShippingCost,
                total AS Total,
                currency AS Currency,
                status AS Status,
                payment_status AS PaymentStatus,
                shipping_method AS ShippingMethod,
                refunded_amount AS RefundedAmount,
                refunded_at AS RefundedAt,
                refund_reason AS RefundReason,
                metadata AS Metadata,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM orders
            WHERE status = @Status
            ORDER BY created_at DESC";

        var orderDtos = await Connection.QueryAsync<OrderDto>(sql, new { Status = status.ToString() });
        return orderDtos.ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetRecentOrdersAsync(int count,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                user_id AS UserId,
                billing_address_id AS BillingAddressId,
                shipping_address_id AS ShippingAddressId,
                payment_method_id AS PaymentMethodId,
                subtotal AS Subtotal,
                tax AS Tax,
                shipping_cost AS ShippingCost,
                total AS Total,
                currency AS Currency,
                status AS Status,
                payment_status AS PaymentStatus,
                shipping_method AS ShippingMethod,
                refunded_amount AS RefundedAmount,
                refunded_at AS RefundedAt,
                refund_reason AS RefundReason,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM orders
            ORDER BY created_at DESC
            LIMIT @Count";

        var orderDtos = await Connection.QueryAsync<OrderDto>(sql, new { Count = count });
        return orderDtos.ToList();
    }

    public async Task<OrderItemDto> GetOrderItemByIdAsync(Guid orderItemId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                order_id AS OrderId,
                product_id AS ProductId,
                variant_id AS VariantId,
                quantity AS Quantity,
                unit_price AS UnitPrice,
                total_price AS TotalPrice,
                currency AS Currency,
                product_data AS ProductData,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM order_items
            WHERE id = @OrderItemId";

        return await Connection.QueryFirstOrDefaultAsync<OrderItemDto>(sql, new { OrderItemId = orderItemId });
    }

    public async Task<IReadOnlyList<OrderDto>> GetByIdsAsync(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<OrderDto>();

        // Convert the IDs to an array for SQL parameters
        var idArray = ids.ToArray();

        const string sql = @"
        SELECT 
            o.id AS Id,
            o.user_id AS UserId,
            o.billing_address_id AS BillingAddressId,
            o.shipping_address_id AS ShippingAddressId,
            o.payment_method_id AS PaymentMethodId,
            o.subtotal AS Subtotal,
            o.tax AS Tax,
            o.shipping_cost AS ShippingCost,
            o.total AS Total,
            o.currency AS Currency,
            o.status AS Status,
            o.payment_status AS PaymentStatus,
            o.shipping_method AS ShippingMethod,
            o.refunded_amount AS RefundedAmount,
            o.refunded_at AS RefundedAt,
            o.refund_reason AS RefundReason,
            o.metadata AS Metadata,
            o.created_at AS CreatedAt,
            o.updated_at AS UpdatedAt
        FROM orders o
        WHERE o.id = ANY(@Ids)
        ORDER BY array_position(@Ids, o.id)";

        var parameters = new { Ids = idArray };
        var orderDtos = await Connection.QueryAsync<OrderDto>(sql, parameters);

        // Extract tracking number from metadata if available for each order
        foreach (var order in orderDtos)
        {
            if (order.Metadata != null && order.Metadata.TryGetValue("trackingNumber", out var trackingNumber))
            {
                order.TrackingNumber = trackingNumber.ToString();
            }
        }

        return orderDtos.ToList();
    }


    public async Task<DataTableResult<OrderDetailDto>> GetOrderDetailDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            return new DataTableResult<OrderDetailDto>(0, "Invalid request");

        try
        {
            // Base query - excluding metadata to avoid casting issues, will load separately
            var selectSql = new StringBuilder(@"
            SELECT 
                o.id AS Id,
                o.user_id AS UserId,
                o.billing_address_id AS BillingAddressId,
                o.shipping_address_id AS ShippingAddressId,
                o.payment_method_id AS PaymentMethodId,
                o.subtotal AS Subtotal,
                o.tax AS Tax,
                o.shipping_cost AS ShippingCost,
                o.total AS Total,
                o.currency AS Currency,
                o.status AS Status,
                o.payment_status AS PaymentStatus,
                o.shipping_method AS ShippingMethod,
                o.refunded_amount AS RefundedAmount,
                o.refunded_at AS RefundedAt,
                o.refund_reason AS RefundReason,
                o.created_by AS CreatedBy,
                o.modified_by AS ModifiedBy,
                o.last_modified AS LastModified,
                o.created_at AS CreatedAt,
                o.updated_at AS UpdatedAt
            FROM orders o
            LEFT JOIN users u ON o.user_id = u.id");

            // Count query
            const string countSql = "SELECT COUNT(*) FROM orders o LEFT JOIN users u ON o.user_id = u.id";

            // Where clause for filtering
            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            // Apply global search if provided
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                whereClause.Append(" WHERE (");
                whereClause.Append("u.email ILIKE @SearchValue OR ");
                whereClause.Append("u.first_name ILIKE @SearchValue OR ");
                whereClause.Append("u.last_name ILIKE @SearchValue OR ");
                whereClause.Append("o.currency ILIKE @SearchValue OR ");
                whereClause.Append("o.status::text ILIKE @SearchValue OR ");
                whereClause.Append("o.payment_status::text ILIKE @SearchValue OR ");
                whereClause.Append("o.shipping_method ILIKE @SearchValue OR ");
                whereClause.Append("o.refund_reason ILIKE @SearchValue");
                whereClause.Append(")");
                parameters.Add("SearchValue", $"%{request.Search.Value}%");
            }

            // Build ORDER BY clause
            var orderByClause = new StringBuilder(" ORDER BY ");

            if (request.Order != null && request.Order.Any())
            {
                for (int i = 0; i < request.Order.Count; i++)
                {
                    if (i > 0) orderByClause.Append(", ");

                    var order = request.Order[i];
                    if (order.Column < request.Columns.Count)
                    {
                        var column = request.Columns[order.Column];
                        if (column.Orderable)
                        {
                            // Map column names to database columns
                            var dbColumn = column.Data.ToLower() switch
                            {
                                "id" => "o.id",
                                "userid" => "o.user_id",
                                "useremail" => "u.email",
                                "userfullname" => "CONCAT(u.first_name, ' ', u.last_name)",
                                "subtotal" => "o.subtotal",
                                "tax" => "o.tax",
                                "shippingcost" => "o.shipping_cost",
                                "total" => "o.total",
                                "currency" => "o.currency",
                                "status" => "o.status",
                                "paymentstatus" => "o.payment_status",
                                "shippingmethod" => "o.shipping_method",
                                "refundedamount" => "o.refunded_amount",
                                "refundedat" => "o.refunded_at",
                                "refundreason" => "o.refund_reason",
                                "createdat" => "o.created_at",
                                "updatedat" => "o.updated_at",
                                _ => "o.created_at" // Default
                            };

                            orderByClause.Append($"{dbColumn} {(order.IsDescending ? "DESC" : "ASC")}");
                        }
                        else
                        {
                            orderByClause.Append("o.created_at DESC");
                        }
                    }
                    else
                    {
                        orderByClause.Append("o.created_at DESC");
                    }
                }
            }
            else
            {
                orderByClause.Append("o.created_at DESC");
            }

            // Pagination
            var paginationClause = " LIMIT @Length OFFSET @Start";
            parameters.Add("Length", request.Length);
            parameters.Add("Start", request.Start);

            // Build final queries
            var finalCountSql = countSql + whereClause.ToString();
            var finalSelectSql = selectSql.ToString() + whereClause.ToString() + orderByClause.ToString() +
                                 paginationClause;

            // Execute queries
            var totalCount = await Connection.ExecuteScalarAsync<int>(countSql);
            var filteredCount = whereClause.Length > 0
                ? await Connection.ExecuteScalarAsync<int>(finalCountSql, parameters)
                : totalCount;

            var orders = await Connection.QueryAsync<OrderDetailDto>(finalSelectSql, parameters);
            var orderList = orders.ToList();

            // Now load additional data for each order (similar to your existing pattern)
            foreach (var order in orderList)
            {
                // Load metadata separately to extract tracking number
                const string metadataSql = "SELECT metadata FROM orders WHERE id = @OrderId";
                var metadataJson =
                    await Connection.QueryFirstOrDefaultAsync<string>(metadataSql, new { OrderId = order.Id });

                if (!string.IsNullOrEmpty(metadataJson))
                {
                    try
                    {
                        var metadata =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
                        order.Metadata = metadata;

                        // Extract tracking number
                        if (metadata != null && metadata.TryGetValue("trackingNumber", out var trackingNumber))
                        {
                            order.TrackingNumber = trackingNumber?.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Failed to parse metadata for order {OrderId}", order.Id);
                    }
                }

                // Load user information separately if needed for datatable
                if (order.UserId.HasValue)
                {
                    const string userSql = @"
                    SELECT 
                        id AS Id,
                        email AS Email,
                        first_name AS FirstName,
                        last_name AS LastName,
                        middle_name AS MiddleName,
                        phone AS Phone,
                        role AS Role,
                        is_active AS IsActive,
                        last_login AS LastLogin,
                        email_verified AS EmailVerified,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM users
                    WHERE id = @UserId";

                    order.User = await Connection.QueryFirstOrDefaultAsync<UserDto>(
                        userSql, new { UserId = order.UserId.Value });
                }

                // For datatable, we might want to load only essential related data
                // Load shipping address if needed for display
                if (order.ShippingAddressId.HasValue)
                {
                    const string shippingAddressSql = @"
                    SELECT 
                        id AS Id,
                        user_id AS UserId,
                        address_line1 AS AddressLine1,
                        address_line2 AS AddressLine2,
                        city AS City,
                        state AS State,
                        postal_code AS PostalCode,
                        country AS Country,
                        phone AS Phone,
                        is_default AS IsDefault,
                        address_type AS AddressType,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM addresses
                    WHERE id = @AddressId";

                    order.ShippingAddress = await Connection.QueryFirstOrDefaultAsync<AddressDto>(
                        shippingAddressSql, new { AddressId = order.ShippingAddressId.Value });
                }

                // Load billing address if needed for display
                if (order.BillingAddressId.HasValue)
                {
                    const string billingAddressSql = @"
                    SELECT 
                        id AS Id,
                        user_id AS UserId,
                        address_line1 AS AddressLine1,
                        address_line2 AS AddressLine2,
                        city AS City,
                        state AS State,
                        postal_code AS PostalCode,
                        country AS Country,
                        phone AS Phone,
                        is_default AS IsDefault,
                        address_type AS AddressType,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM addresses
                    WHERE id = @AddressId";

                    order.BillingAddress = await Connection.QueryFirstOrDefaultAsync<AddressDto>(
                        billingAddressSql, new { AddressId = order.BillingAddressId.Value });
                }

                // Load payment method if needed for display
                if (order.PaymentMethodId.HasValue)
                {
                    const string paymentMethodSql = @"
                    SELECT 
                        id AS Id,
                        user_id AS UserId,
                        type AS Type,
                        provider AS Provider,
                        display_name AS DisplayName,
                        card_brand AS CardBrand,
                        last_four_digits AS LastFourDigits,
                        expiry_date AS ExpiryDate,
                        is_default AS IsDefault,
                        is_active AS IsActive,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM payment_methods
                    WHERE id = @PaymentMethodId";

                    order.PaymentMethod = await Connection.QueryFirstOrDefaultAsync<PaymentMethodDto>(
                        paymentMethodSql, new { PaymentMethodId = order.PaymentMethodId.Value });
                }

                // Load payments for this order
                const string paymentsSql = @"
                SELECT 
                    id AS Id,
                    order_id AS OrderId,
                    user_id AS UserId,
                    amount AS Amount,
                    currency AS Currency,
                    method AS MethodType,
                    provider AS Provider,
                    status AS Status,
                    external_reference AS ExternalReference,
                    transaction_id AS TransactionId,
                    payment_method_id AS PaymentMethodId,
                    processed_at AS ProcessedAt,
                    error_message AS ErrorMessage,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM payments
                WHERE order_id = @OrderId";

                var payments = await Connection.QueryAsync<PaymentDto>(paymentsSql, new { OrderId = order.Id });
                order.Payments = payments.ToList();

                // Load order items for this order (excluding product_data to avoid casting issues)
                const string itemsSql = @"
                SELECT 
                    id AS Id,
                    order_id AS OrderId,
                    product_id AS ProductId,
                    variant_id AS VariantId,
                    quantity AS Quantity,
                    unit_price AS UnitPrice,
                    total_price AS TotalPrice,
                    currency AS Currency,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM order_items
                WHERE order_id = @OrderId";

                var items = await Connection.QueryAsync<OrderItemDto>(itemsSql, new { OrderId = order.Id });
                var itemsList = items.ToList();

                // Load product_data separately for each item
                foreach (var item in itemsList)
                {
                    const string productDataSql = "SELECT product_data FROM order_items WHERE id = @ItemId";
                    var productDataJson =
                        await Connection.QueryFirstOrDefaultAsync<string>(productDataSql, new { ItemId = item.Id });

                    if (!string.IsNullOrEmpty(productDataJson))
                    {
                        try
                        {
                            var productData =
                                JsonSerializer
                                    .Deserialize<Dictionary<string, object>>(productDataJson);
                            item.ProductData = productData;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Failed to parse product_data for order item {ItemId}", item.Id);
                        }
                    }
                }

                order.Items = itemsList;
            }

            Logger.LogInformation("Retrieved {Count} orders for datatable with pagination {Start}-{End}",
                orderList.Count, request.Start, request.Start + request.Length);

            return new DataTableResult<OrderDetailDto>(
                request.Draw,
                totalCount,
                filteredCount,
                orderList);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving orders for datatable");
            return new DataTableResult<OrderDetailDto>(request.Draw, $"Error retrieving orders: {ex.Message}");
        }
    }
}