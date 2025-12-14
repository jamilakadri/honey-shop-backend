using Microsoft.EntityFrameworkCore.Storage;
using MielShop.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MielShop.API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IProductRepository Products { get; }
        public IProductImageRepository ProductImages { get; }

        public ICategoryRepository Categories { get; }
        public IOrderRepository Orders { get; }
        public ICartRepository Carts { get; }
        public ICartItemRepository CartItems { get; }
        public IReviewRepository Reviews { get; }
        public IWishlistRepository Wishlists { get; }
        public IAddressRepository Addresses { get; }
        public IPromoCodeRepository PromoCodes { get; }
        public IUserRepository Users { get; }
        public IOrderItemRepository OrderItems { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            ProductImages = new ProductImageRepository(_context);
            Products = new ProductRepository(_context);
            
            Categories = new CategoryRepository(_context);
            Orders = new OrderRepository(_context);
            Carts = new CartRepository(_context);
            CartItems = new CartItemRepository(_context);
            Reviews = new ReviewRepository(_context);
            Wishlists = new WishlistRepository(_context);
            Addresses = new AddressRepository(_context);
            PromoCodes = new PromoCodeRepository(_context);
            Users = new UserRepository(_context);
            OrderItems = new OrderItemRepository(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        public async Task<int> CountAsync<T>() where T : class
        {
            return await _context.Set<T>().CountAsync();
        }

        public async Task<int> CountAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().Where(predicate).CountAsync();
        }
    }
}