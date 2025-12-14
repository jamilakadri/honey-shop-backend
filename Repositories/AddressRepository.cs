using MielShop.API.Data;
using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        public AddressRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}