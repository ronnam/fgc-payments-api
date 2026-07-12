using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.DTOS;
using Fgc.Payments.Domain.Enums;
using Fgc.Payments.Infraestructure.Persistence;
using Fgc.Payments.IntegrationTests.Infraestructure;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Fgc.Payments.IntegrationTests.Controllers
{
    public class PaymentsApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        public PaymentsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _factory = factory;

            ResetDatabase();
        }

        private void ResetDatabase() 
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private static StringContent CreateJsonContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static string GenerateAdminToken()
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("FgcUsers-Auth-JWT-Key-2026-Strong-And-Secure")
            );

            var credencials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: "Fgc.UsersAPI",
                audience: "Fgc.PaymentsAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credencials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [Fact]
        public async Task GetPaymentById_WhenPaymentExists_ShouldReturn200WithPayment()
        {
            // Arrange
            var token = GenerateAdminToken();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new PaymentRequest(
                OrderId: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                GameId: Guid.NewGuid(),
                Amount: 59.90m
            );

            var createResponse = await _client.PostAsJsonAsync("/api/payments", request);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentResponse>();
            Assert.NotNull(createdPayment);

            // Act
            var response = await _client.GetAsync($"/api/payments/{createdPayment.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<PaymentResponse>();
            Assert.NotNull(body);
            Assert.Equal(createdPayment.Id, body.Id);
            Assert.Equal("Approved", body.Status);
        }

        [Fact]
        public async Task CreatePayment_WhenValidRequest_ShouldReturn201WithApprovedStatus()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", GenerateAdminToken());

            var request = new PaymentRequest(
                OrderId: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                GameId: Guid.NewGuid(),
                Amount: 59.90m
            );

            var content = CreateJsonContent(request);

            // Act
            var response = await _client.PostAsync("/api/payments", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>();
            Assert.NotNull (payment);
            Assert.Equal("Approved", payment!.Status);
            Assert.Equal(request.UserId, payment.UserId);
            Assert.Equal(request.GameId, payment.GameId);
            Assert.Equal(request.Amount, payment.Amount);
        }

        [Fact]
        public async Task GetPaymentById_WhenPaymentDoesNotExist_ShouldReturn404()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GenerateAdminToken());

            var response = await _client.GetAsync($"/api/payments/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPaymentById_WhenUnauthenticated_ShouldReturn401()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.GetAsync($"/api/payments/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreatePayment_WhenAmountIsNegative_ShouldReturn400()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GenerateAdminToken());

            var request = new PaymentRequest(
                OrderId: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                GameId: Guid.NewGuid(),
                Amount: -10m
            );

            var content = CreateJsonContent(request);

            // Act
            var response = await _client.PostAsync("/api/payments", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreatePayment_WhenDuplicateOrderId_ShouldReturn400()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GenerateAdminToken());

            var orderId = Guid.NewGuid();
            var firstRequest = new PaymentRequest(
                OrderId: orderId,
                UserId: Guid.NewGuid(),
                GameId: Guid.NewGuid(),
                Amount: 39.99m
            );

            var firstContent = CreateJsonContent(firstRequest);

            // Act - first creation should succeed
            var firstResponse = await _client.PostAsync("/api/payments", firstContent);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            // Arrange - duplicate request with the same OrderId
            var duplicateRequest = new PaymentRequest(
                OrderId: orderId,
                UserId: Guid.NewGuid(),
                GameId: Guid.NewGuid(),
                Amount: 39.99m
            );

            var duplicateContent = CreateJsonContent(duplicateRequest);

            // Act - second creation with the same OrderId should fail
            var duplicateResponse = await _client.PostAsync("/api/payments", duplicateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
        }

        [Fact]
        public async Task Consume_OrderPlacedEvent_ShouldPersistApprovedPayment()
        {
            // Arrange
            var harness = _factory.Services.GetRequiredService<ITestHarness>();
            await harness.Start();

            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var price = 59.90m;

            var orderPlacedEvent = new OrderPlacedEvent(orderId, userId, gameId, price);

            // Act
            await harness.Bus.Publish(orderPlacedEvent);

            // Assert
            Assert.True(await harness.Consumed.Any<OrderPlacedEvent>(
                x => x.Context.Message.OrderId == orderId));

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

            var payment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id ==  orderId);

            Assert.NotNull(payment);
            Assert.Equal(userId, payment.UserId);
            Assert.Equal(gameId, payment.GameId);
            Assert.Equal(price, payment.Amount);
            Assert.Equal(PaymentStatus.Approved, payment.Status);
        }
    }
}
