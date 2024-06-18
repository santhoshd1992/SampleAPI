using SampleAPI.Entities;
using SampleAPI.Requests;

namespace SampleAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetRecentOrdersAsync();
        Task<Order> AddOrderAsync(Order order);
        Task<IEnumerable<Order>> GetOrdersAfterDaysAsync(int days);
    }
}
