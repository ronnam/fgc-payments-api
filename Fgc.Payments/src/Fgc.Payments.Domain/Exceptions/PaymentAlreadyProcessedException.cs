using Fgc.Payments.Domain.Enums;

namespace Fgc.Payments.Domain.Exceptions
{
    public class PaymentAlreadyProcessedException : Exception
    {
        public PaymentAlreadyProcessedException(Guid paymentId, PaymentStatus currentStatus) 
            : base($"Payment with ID {paymentId} has already been processed. Current status: {currentStatus}.")
        {
        }
    }
}
