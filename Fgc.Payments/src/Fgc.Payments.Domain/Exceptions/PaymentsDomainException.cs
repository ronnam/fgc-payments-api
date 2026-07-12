namespace Fgc.Payments.Domain.Exceptions
{
    public class PaymentsDomainException : Exception
    {
        public PaymentsDomainException(string message) : base(message) { }
    }
}
