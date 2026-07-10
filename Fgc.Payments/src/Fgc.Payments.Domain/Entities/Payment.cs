
using Fgc.Payments.Domain.Enums;
using Fgc.Payments.Domain.Exceptions;

namespace Fgc.Payments.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        private Payment() { }

        private Payment(Guid orderId, Guid userId, Guid gameId, decimal amount)
        {
            Id = orderId;
            UserId = userId;
            GameId = gameId;
            Amount = amount;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Payment Create(Guid orderId, Guid userId, Guid gameId, decimal amount)
        {
            Validate(userId, gameId, amount);
            return new Payment(orderId, userId, gameId, amount);
        }

        private static void Validate(Guid userId, Guid gameId, decimal amount)
        {
            if (userId == Guid.Empty)
                throw new PaymentValidationException("User is required.");
            if (gameId == Guid.Empty)
                throw new PaymentValidationException("Game is required.");
            if (amount <= 0)
                throw new PaymentValidationException("Amount must be greater than zero.");
        }

        public void Approve()
        {
            if (Status != PaymentStatus.Pending)
                throw new PaymentAlreadyProcessedException(Id, Status);
            Status = PaymentStatus.Approved;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            if (Status != PaymentStatus.Pending)
                throw new PaymentAlreadyProcessedException(Id, Status);
            Status = PaymentStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
        }

    }
}
