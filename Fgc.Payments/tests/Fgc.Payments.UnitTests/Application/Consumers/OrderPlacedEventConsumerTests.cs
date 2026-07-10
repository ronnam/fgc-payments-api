using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Consumers;
using Fgc.Payments.Application.DTOS;
using Fgc.Payments.Application.Interfaces;
using MassTransit;
using Moq;

namespace Fgc.Payments.UnitTests.Application.Consumers
{
    public class OrderPlacedEventConsumerTests
    {
        private readonly Mock<IProcessPaymentUseCase> _useCaseMock = new();
        private readonly OrderPlacedEventConsumer _consumer;

        public OrderPlacedEventConsumerTests()
        {
            _consumer = new OrderPlacedEventConsumer(_useCaseMock.Object);
        }

        [Fact]
        public async Task Consume_ShouldCallProcessAsync()
        {
            var orderEvent = new OrderPlacedEvent(
                OrderId: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                GameId: Guid.NewGuid(),
                Price: 59.99m
            );

            var context = Mock.Of<ConsumeContext<OrderPlacedEvent>>(c => c.Message == orderEvent);

            await _consumer.Consume(context);

            _useCaseMock.Verify(
                u => u.ProcessAsync(It.Is<ProcessPaymentCommand>(c =>
                    c.OrderId == orderEvent.OrderId &&
                    c.UserId == orderEvent.UserId &&
                    c.GameId == orderEvent.GameId &&
                    c.Amount == orderEvent.Price)),
                Times.Once()
            );
        }
    }
}
