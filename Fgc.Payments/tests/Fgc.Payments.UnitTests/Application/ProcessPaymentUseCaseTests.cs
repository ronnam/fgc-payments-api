using Fgc.Payments.Application.DTOS;
using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Application.Services;
using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Domain.Enums;
using MassTransit;
using Moq;

namespace Fgc.Payments.UnitTests.Application;

public class ProcessPaymentUseCaseTests
{
    private readonly Mock<IPaymentRepository> _repositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly ProcessPaymentUseCase _useCase;

    public ProcessPaymentUseCaseTests()
    {
        _useCase = new ProcessPaymentUseCase(
            _repositoryMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_WhenSuccessful_ShouldApproveAndPublishEvent()
    {
        var command = new ProcessPaymentCommand(
            OrderId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            GameId: Guid.NewGuid(),
            Amount: 59.90m);

        Payment? capturedPayment = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p)
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        var result = await _useCase.ProcessAsync(command);

        Assert.NotNull(capturedPayment);
        Assert.Equal(command.UserId, capturedPayment.UserId);
        Assert.Equal(command.GameId, capturedPayment.GameId);
        Assert.Equal(command.Amount, capturedPayment.Amount);

        Assert.Equal(PaymentStatus.Approved, capturedPayment.Status);
        Assert.NotNull(capturedPayment.ProcessedAt);

        Assert.Equal(capturedPayment.Id, result.Id);
        Assert.Equal(PaymentStatus.Approved, result.Status);

        _publishEndpointMock.Verify(p => p.Publish(
                It.Is<PaymentProcessedEvent>(e =>
                    e.OrderedId == command.OrderId &&
                    e.UserId == command.UserId &&
                    e.GameId == command.GameId &&
                    e.Price == command.Amount &&
                    e.Status == "Approved"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenRepositoryFails_ShouldThrow()
    {
        var command = new ProcessPaymentCommand(
            OrderId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            GameId: Guid.NewGuid(),
            Amount: 29.90m);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var exception = await Assert.ThrowsAsync<Exception>(
            () => _useCase.ProcessAsync(command));

        Assert.Contains("Database connection failed", exception.Message);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Payment>()),
            Times.Never);

        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PaymentProcessedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}