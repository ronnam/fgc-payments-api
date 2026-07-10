using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Domain.Enums;
using Fgc.Payments.Domain.Exceptions;

namespace Fgc.Payments.UnitTests.Domain;

public class PaymentTests
{
    [Fact]
    public void CreatePayment_ShouldSetProperties()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var amount = 59.90m;

        var payment = Payment.Create(orderId, userId, gameId, amount);

        Assert.Equal(orderId, payment.Id);
        Assert.Equal(userId, payment.UserId);
        Assert.Equal(gameId, payment.GameId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.NotEqual(default, payment.CreatedAt);
        Assert.Null(payment.ProcessedAt);
    }

    [Fact]
    public void Approve_ShouldSetStatusToApproved()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var amount = 29.90m;

        var payment = Payment.Create(orderId, userId, gameId, amount);
        payment.Approve();
        Assert.Equal(PaymentStatus.Approved, payment.Status);
        Assert.NotNull(payment.ProcessedAt);
    }

    [Fact]
    public void Reject_ShouldSetStatusToRejected()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var amount = 29.90m;

        var payment = Payment.Create(orderId, userId, gameId, amount);
        payment.Reject();
        Assert.Equal(PaymentStatus.Rejected, payment.Status);
        Assert.NotNull(payment.ProcessedAt);
    }

    [Fact]
    public void Approve_WhenAlreadyProcessed_ShouldThrow()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var amount = 29.90m;

        var payment = Payment.Create(orderId, userId, gameId, amount);
        payment.Approve();
        Assert.Throws<PaymentAlreadyProcessedException>(() => payment.Approve());
    }

    [Fact]
    public void Reject_WhenAlreadyProcessed_ShouldThrow()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var amount = 29.90m;

        var payment = Payment.Create(orderId, userId, gameId, amount);
        payment.Reject();
        Assert.Throws<PaymentAlreadyProcessedException>(() => payment.Reject());
    }
}