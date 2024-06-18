using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleAPI.Controllers;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;

namespace SampleAPI.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderRepository> _mockRepo;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockRepo = new Mock<IOrderRepository>();
            _controller = new OrdersController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetRecentOrders_ReturnsRecentOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { EntryDate = DateTime.UtcNow.AddHours(-1), Name = "Order1", Description = "Description1" },
                new Order { EntryDate = DateTime.UtcNow.AddDays(-2), Name = "Order2", Description = "Description2" }
            };
            _mockRepo.Setup(repo => repo.GetRecentOrdersAsync())
                .ReturnsAsync(orders.Where(o => o.EntryDate >= DateTime.UtcNow.AddDays(-1)));

            // Act
            var result = await _controller.GetRecentOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrders = Assert.IsAssignableFrom<IEnumerable<Order>>(okResult.Value);
            Assert.Single(returnedOrders);
        }

        [Fact]
        public async Task GetRecentOrders_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetRecentOrdersAsync())
                .ThrowsAsync(new ApplicationException("Error getting recent orders"));

            // Act
            var result = await _controller.GetRecentOrders();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task SubmitOrder_ReturnsCreatedOrder()
        {
            // Arrange
            var newOrder = new Order { Name = "New Order", Description = "New Description" };
            _mockRepo.Setup(repo => repo.AddOrderAsync(It.IsAny<Order>())).ReturnsAsync(newOrder);

            // Act
            var result = await _controller.SubmitOrder(newOrder);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var order = Assert.IsType<Order>(createdAtActionResult.Value);
            Assert.Equal("New Order", order.Name);
            Assert.Equal("New Description", order.Description);
            Assert.False(order.IsDeleted);
            Assert.True(order.IsInvoiced);
        }

        [Fact]
        public async Task SubmitOrder_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var newOrder = new Order { Name = "New Order", Description = "New Description" };
            _mockRepo.Setup(repo => repo.AddOrderAsync(It.IsAny<Order>()))
                .ThrowsAsync(new ApplicationException("Error adding new order"));

            // Act
            var result = await _controller.SubmitOrder(newOrder);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task SubmitOrder_ReturnsBadRequest_OnInvalidModel()
        {
            // Arrange
            var invalidOrder = new Order { Name = "", Description = "Description" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.SubmitOrder(invalidOrder);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task GetOrdersAfterDays_ReturnsOrders()
        {
            // Arrange
            var days = 5;
            var orders = new List<Order>
            {
                new Order { EntryDate = DateTime.UtcNow.AddDays(-2), Name = "Order1", Description = "Description1" },
                new Order { EntryDate = DateTime.UtcNow.AddDays(-4), Name = "Order2", Description = "Description2" }
            };
            _mockRepo.Setup(repo => repo.GetOrdersAfterDaysAsync(days))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrdersAfterDays(days);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrders = Assert.IsAssignableFrom<IEnumerable<Order>>(okResult.Value);
            Assert.Equal(2, returnedOrders.Count());
        }

        [Fact]
        public async Task GetOrdersAfterDays_ReturnsBadRequest_ForNegativeDays()
        {
            // Act
            var result = await _controller.GetOrdersAfterDays(-5);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Days must be a non-negative number.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetOrdersAfterDays_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var days = 5;
            _mockRepo.Setup(repo => repo.GetOrdersAfterDaysAsync(days))
                .ThrowsAsync(new ApplicationException("Error getting orders after days"));

            // Act
            var result = await _controller.GetOrdersAfterDays(days);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
