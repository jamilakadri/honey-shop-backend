using MielShop.API.Models;
using MielShop.API.Repositories;
using MielShop.API.DTOs.Order;

namespace MielShop.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId) // Changé de string à int
        {
            return await _unitOfWork.Orders.GetOrdersByUserAsync(userId);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
        }

        public async Task<Order> CreateOrderAsync(int userId, CreateOrderDto dto) // Changé de string à int
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Récupérer le panier (userId est maintenant int)
                var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    throw new Exception("Le panier est vide");
                }

                // 2. Vérifier les adresses
                var shippingAddress = await _unitOfWork.Addresses.GetByIdAsync(dto.ShippingAddressId);
                var billingAddress = await _unitOfWork.Addresses.GetByIdAsync(dto.BillingAddressId);

                if (shippingAddress == null || billingAddress == null)
                {
                    throw new Exception("Adresse invalide");
                }

                // 3. Calculer les montants
                decimal subtotal = cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
                decimal shippingCost = 7.00m;
                decimal tax = subtotal * 0.19m;
                decimal discount = 0;

                // 4. Appliquer code promo si fourni
                if (!string.IsNullOrEmpty(dto.PromoCode))
                {
                    var promo = await _unitOfWork.PromoCodes.GetActivePromoByCodeAsync(dto.PromoCode);
                    if (promo != null && promo.UsageCount < promo.UsageLimit)
                    {
                        if (promo.DiscountType == "Percentage")
                        {
                            discount = subtotal * (promo.DiscountValue / 100);
                        }
                        else if (promo.DiscountType == "Fixed")
                        {
                            discount = promo.DiscountValue;
                        }

                        if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
                        {
                            discount = promo.MaxDiscountAmount.Value;
                        }
                    }
                }

                decimal totalAmount = subtotal + shippingCost + tax - discount;

                // 5. Créer la commande
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    UserId = userId,
                    BillingAddressId = dto.BillingAddressId,
                    BillingFullName = billingAddress.FullName,
                    BillingEmail = user?.Email ?? "email@example.com",
                    BillingPhone = billingAddress.PhoneNumber,
                    BillingAddress = FormatAddress(billingAddress),
                    ShippingAddressId = dto.ShippingAddressId,
                    ShippingFullName = shippingAddress.FullName,
                    ShippingPhone = shippingAddress.PhoneNumber,
                    ShippingAddress = FormatAddress(shippingAddress),
                    SubTotal = subtotal,
                    ShippingCost = shippingCost,
                    Tax = tax,
                    DiscountAmount = discount,
                    TotalAmount = totalAmount,
                    OrderStatus = "Pending",
                    PaymentStatus = "Pending",
                    Notes = dto.Notes,
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // 6. Créer les items et réduire le stock
                foreach (var cartItem in cart.CartItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);

                    if (product == null || product.StockQuantity < cartItem.Quantity)
                    {
                        throw new Exception($"Stock insuffisant pour {product?.Name}");
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name,
                        ProductSKU = product.SKU,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price,
                        TotalPrice = cartItem.Price * cartItem.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.OrderItems.AddAsync(orderItem);

                    // Réduire le stock
                    product.StockQuantity -= cartItem.Quantity;
                    _unitOfWork.Products.Update(product);
                }

                // 7. Vider le panier
                _unitOfWork.CartItems.DeleteRange(cart.CartItems);

                // 8. Sauvegarder tout
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return await GetOrderByIdAsync(order.OrderId) ?? order;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId) // Changé de string à int
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            if (order == null || order.UserId != userId) return false;

            if (order.OrderStatus != "Pending" && order.OrderStatus != "Processing")
            {
                throw new Exception("Cette commande ne peut plus être annulée");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Remettre les produits en stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId ?? 0);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _unitOfWork.Products.Update(product);
                    }
                }

                order.OrderStatus = "Cancelled";
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Orders.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null) return false;

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == "Shipped")
            {
                order.ShippedAt = DateTime.UtcNow;
            }
            else if (status == "Delivered")
            {
                order.DeliveredAt = DateTime.UtcNow;
            }

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _unitOfWork.Orders.GetAllAsync();
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await _unitOfWork.Orders.FindAsync(o =>
                o.OrderStatus == "Pending" || o.OrderStatus == "Processing");
        }

        public async Task<object> GetOrderStatisticsAsync()
        {
            var totalOrders = await _unitOfWork.Orders.CountAsync();
            var pendingOrders = await _unitOfWork.Orders.CountAsync(o => o.OrderStatus == "Pending");
            var completedOrders = await _unitOfWork.Orders.CountAsync(o => o.OrderStatus == "Completed");

            var allOrders = await _unitOfWork.Orders.GetAllAsync();
            var totalRevenue = allOrders
                .Where(o => o.OrderStatus == "Completed")
                .Sum(o => o.TotalAmount);

            return new
            {
                totalOrders,
                pendingOrders,
                completedOrders,
                totalRevenue
            };
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        private string FormatAddress(Address address)
        {
            return $"{address.AddressLine1}, {address.City}, {address.PostalCode}, {address.Country}";
        }
    }
}