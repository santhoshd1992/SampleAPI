using Microsoft.AspNetCore.Mvc;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;

namespace SampleAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        // Add more dependencies as needed.

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet("recent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Order>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<List<Order>>> GetRecentOrders()
        {
            try
            {
                var recentOrders = await _orderRepository.GetRecentOrdersAsync();

                if (recentOrders == null || !recentOrders.Any())
                {
                    return NotFound("No recent orders found");
                }

                return Ok(recentOrders);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Order), 201)]
        [ProducesResponseType(typeof(SerializableError), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Order>> SubmitOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                order.EntryDate = DateTime.UtcNow;
                var createdOrder = await _orderRepository.AddOrderAsync(order);
                return CreatedAtAction(nameof(GetRecentOrders), new { id = createdOrder.Id }, createdOrder);
            }
            catch (ApplicationException ex)
            {                
                return StatusCode(500, "An error occurred while submitting the order.");
            }
        }

        [HttpGet("afterdays/{days}")]
        [ProducesResponseType(typeof(IEnumerable<Order>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersAfterDays(int days)
        {
            if (days < 0)
            {
                return BadRequest("Days must be a non-negative number.");
            }

            try
            {
                var orders = await _orderRepository.GetOrdersAfterDaysAsync(days);
                return Ok(orders);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, "An error occurred while getting orders.");
            }
        }

        // Handle exceptions
        [HttpGet("error")]
        public IActionResult TestError()
        {
            throw new Exception("This is a test exception.");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/error")]
        public IActionResult HandleError() =>
            Problem();
    }
}
