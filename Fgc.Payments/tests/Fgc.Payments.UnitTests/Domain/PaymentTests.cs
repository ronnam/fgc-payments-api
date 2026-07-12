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
    public void CreatePayment_ShouldThrow_WhenUserIdIsEmpty()
    {
        var ex = Assert.Throws<PaymentValidationException>(() =>
            Payment.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 59.90m));

        Assert.Equal("User is required.", ex.Message);
    }

    [Fact]
    public void CreatePayment_ShouldThrow_WhenGameIdIsEmpty()
    {
        var ex = Assert.Throws<PaymentValidationException>(() =>
            Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 59.90m));
        Assert.Equal("Game is required.", ex.Message);
    }

    [Fact]
    public void CreatePayment_ShouldThrow_WhenAmountIsNotPositive()
    {
        var ex = Assert.Throws<PaymentValidationException>(() =>
            Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -59.90m));
        Assert.Equal("Amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void CreatePayment_ShouldThrow_WhenAmountIsZero()
    {
        var ex = Assert.Throws<PaymentValidationException>(() =>
            Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m));
        Assert.Equal("Amount must be greater than zero.", ex.Message);
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
    public void Approve_ShouldChangeStatusToApproved()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100);

        payment.Approve();

        Assert.Equal(PaymentStatus.Approved, payment.Status);
        Assert.NotNull(payment.ProcessedAt);
    }

    [Fact]
    public void Approve_ShouldThrow_WhenAlreadyApproved()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100);

        payment.Approve();

        var ex = Assert.Throws<PaymentAlreadyProcessedException>(() => payment.Approve());

        Assert.Equal(payment.Id, ex.PaymentId);
        Assert.Equal(PaymentStatus.Approved, ex.CurrentStatus);
    }

    [Fact]
    public void Approve_ShouldThrow_WhenAlreadyRejected()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100);

        payment.Reject();

        var ex = Assert.Throws<PaymentAlreadyProcessedException>(() => payment.Approve());

        Assert.Equal(payment.Id, ex.PaymentId);
    }

    [Fact]
    public void Reject_ShouldChangeStatusToRejected()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100);

        payment.Reject();

        Assert.Equal(PaymentStatus.Rejected, payment.Status);
        Assert.NotNull(payment.ProcessedAt);
    }

    [Fact]
    public void Reject_ShouldThrow_WhenAlreadyProcessed()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100);

        payment.Approve();

        var ex = Assert.Throws<PaymentAlreadyProcessedException>(() => payment.Reject());

        Assert.Equal(payment.Id, ex.PaymentId);
        Assert.Equal(PaymentStatus.Approved, ex.CurrentStatus);
    }

    [Fact]
    public void Reject_ShouldThrow_WhenAlreadyRejected()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100);
        payment.Reject();

        var ex = Assert.Throws<PaymentAlreadyProcessedException>(() => payment.Reject());

        Assert.Equal(payment.Id, ex.PaymentId);
        Assert.Equal(PaymentStatus.Rejected, ex.CurrentStatus);
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