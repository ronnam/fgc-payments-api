using Fgc.Payments.Infraestructure.Persistence;
using Fgc.Payments.IntegrationTests.Infraestructure;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using MassTransit.Testing;
using Fgc.MessageContracts.Events;

namespace Fgc.Payments.IntegrationTests.Controllers
{
    public class PaymentsFlowTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PaymentsFlowTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            ResetDatabase();
        }

        private void ResetDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [Fact]
        public async Task ProcessPayment_WhenOrderPlacedEventReceived_ShouldApproveAndPersist()
        {
            // Arrange
            var testHarness = _factory.Services.GetRequiredService<ITestHarness>();
            await testHarness.Start();

            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            const decimal price = 59.90m;

            var orderPlacedEvent = new OrderPlacedEvent(orderId, userId, gameId, price);

            // Act - publica o evento como se o CatalogAPI tivesse enviado.
            await testHarness.Bus.Publish(orderPlacedEvent);

            // Assert 1 - o consumer consumiu a mensagem?
            var consumed = await testHarness.Consumed.Any<OrderPlacedEvent>();
            Assert.True(consumed, "OrderPlacedEvent deveria ter sido consumido.");

            // Assert 2 - o PaymentProcessedEvento foi publicado?
            var publishedMessage = await testHarness.Published
                .SelectAsync<PaymentProcessedEvent>()
                .FirstOrDefault();

            Assert.NotNull(publishedMessage);
            Assert.Equal("Approved", publishedMessage.Context.Message.Status);

            // Assert 3 - o pagamento foi salvo no banco como Approved?
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var payment = await context.Payments.FindAsync(orderId);

            Assert.NotNull(payment);
            Assert.Equal("Approved", payment.Status.ToString());
            Assert.NotNull(payment.ProcessedAt);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task ProcessPayment_WhenAmountIsInvalid_ShouldPublishFault(decimal invalidAmount)
        {
            var testHarness = _factory.Services.GetRequiredService<ITestHarness>();
            await testHarness.Start();

            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            await testHarness.Bus.Publish(new OrderPlacedEvent(orderId, userId, gameId, invalidAmount));

            var fault = await testHarness.Published.SelectAsync<Fault<OrderPlacedEvent>>().Any();
            Assert.True(fault, "Should have published Fault for invalid amount.");

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var payment = await context.Payments.FindAsync(orderId);
            Assert.Null(payment);
        }

        [Fact]
        public async Task ProcessPayment_WhenUserIdIsEmpty_ShouldPublishFault()
        {
            var testHarness = _factory.Services.GetRequiredService<ITestHarness>();
            await testHarness.Start();

            var orderId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            const decimal price = 59.90m;

            await testHarness.Bus.Publish(new OrderPlacedEvent(orderId, Guid.Empty, gameId, price));

            var fault = await testHarness.Published.SelectAsync<Fault<OrderPlacedEvent>>().Any();
            Assert.True(fault, "Should have published Fault for empty userId");

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var payment = await context.Payments.FindAsync(orderId);
            Assert.Null(payment);
        }

        [Fact]
        public async Task ProcessPayment_WhenGameIdIsEmpty_ShouldPublishFault()
        {
            var testHarness = _factory.Services.GetRequiredService<ITestHarness>();
            await testHarness.Start();

            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            const decimal price = 59.90m;

            await testHarness.Bus.Publish(new OrderPlacedEvent(orderId, userId, Guid.Empty, price));

            var fault = await testHarness.Published.SelectAsync<Fault<OrderPlacedEvent>>().Any();
            Assert.True(fault, "Should have published Fault for empty gameId");

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var payment = await context.Payments.FindAsync(orderId);
            Assert.Null(payment);
        }
    }
}
