namespace Fgc.Payments.Domain.Exceptions
{
    public class PaymentValidationException(string message) : PaymentsDomainException(message)    
    {
    }
}
