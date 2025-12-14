namespace MielShop.API.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IProductImageRepository ProductImages { get; } // ADD THIS LINE
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        ICartRepository Carts { get; }
        ICartItemRepository CartItems { get; }
        IReviewRepository Reviews { get; }
        IWishlistRepository Wishlists { get; }
        IAddressRepository Addresses { get; } // AJOUT
        IPromoCodeRepository PromoCodes { get; }
        IUserRepository Users { get; }
        IOrderItemRepository OrderItems { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> CountAsync<T>() where T : class;
        Task<int> CountAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;

    }
}