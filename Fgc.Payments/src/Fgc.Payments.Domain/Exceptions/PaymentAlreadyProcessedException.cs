using Fgc.Payments.Domain.Enums;

namespace Fgc.Payments.Domain.Exceptions
{
     public class PaymentAlreadyProcessedException(Guid paymentId, PaymentStatus currentStatus) 
        : Exception($"Payment{paymentId} has alredy been processed with status {currentStatus}.")
     {
            public Guid PaymentId => paymentId;
            public PaymentStatus CurrentStatus => currentStatus;
     }
}
