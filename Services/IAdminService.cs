using MielShop.API.Models;

namespace MielShop.API.Services
{
	public interface IAdminService
	{
		Task<object> GetDashboardStatsAsync();
		Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 5);
		Task<IEnumerable<object>> GetTopSellingProductsAsync(int count = 10);
	}
}