using Microsoft.AspNetCore.Mvc;

namespace Fgc.Payments.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Tags("Payments")]
    public class PaymentsController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPaymentById(Guid id)
        {
            return Ok(new { id, status = "approved" });
        }
    }
}
