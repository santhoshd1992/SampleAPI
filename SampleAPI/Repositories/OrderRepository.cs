using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;
using SampleAPI.Requests;

namespace SampleAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SampleApiDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        private readonly List<DateTime> _holidays = new List<DateTime>
        {
            new DateTime(DateTime.Now.Year, 1, 1),  // New Year's Day
            new DateTime(DateTime.Now.Year, 12, 25) // Christmas Day
            // Add more holidays to the list or we can get this from DB also
        };

        public OrderRepository(SampleApiDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-1);
                return await _context.Orders.Where(o => o.EntryDate >= cutoffDate && !o.IsDeleted)
                              .OrderByDescending(o => o.EntryDate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting recent orders: {ex}");
                throw new ApplicationException("Error getting recent orders", ex);
            }
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding new order: {ex}");
                throw new ApplicationException("Error adding new order", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersAfterDaysAsync(int days)
        {
            try
            {
                DateTime startDate = DateTime.UtcNow;
                int daysAdded = 0;
                while (daysAdded < days)
                {
                    startDate = startDate.AddDays(-1);
                    if (!IsWeekend(startDate) && !IsHoliday(startDate))
                    {
                        daysAdded++;
                    }
                }

                return await _context.Orders
                    .Where(o => !o.IsDeleted && o.EntryDate >= startDate)
                    .OrderByDescending(o => o.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting orders after days: {ex}");
                throw new ApplicationException("Error getting orders after days", ex);
            }
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        private bool IsHoliday(DateTime date)
        {
            return _holidays.Contains(date.Date);
        }
    }
}
