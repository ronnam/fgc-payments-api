using Fgc.Payments.Application.DTOS;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fgc.Payments.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Tags("Payments")]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        /// <summary>
        /// Process a new payment
        /// </summary>
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                var response = await paymentService.ProcessAsync(request);

                return CreatedAtAction(nameof(GetPaymentById), new { id = response.Id }, response);
            }
            catch (PaymentsDomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a payment by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentById(Guid id)
        {
            var payment = await paymentService.GetByIdAsync(id);

            if (payment is null)
                return NotFound(new { error = "Payment not found" });

            return Ok(payment);
        }        
    }
}
