using Fgc.Payments.Infraestructure.Persistence;
using Fgc.Payments.IntegrationTests.Infraestructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Fgc.Payments.IntegrationTests.Controllers
{
    public class PaymentsApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        public PaymentsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _factory = factory;

            ResetDatabase();
        }

        private void ResetDatabase() 
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FgcPaymentsDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private static StringContent CreateJsonContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static string GenerateAdminToken()
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("FgcUsers-Auth-JWT-Key-2026-Strong-And-Secure")
            );

            var credencials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: "Fgc.UserAPI",
                audience: "Fgc.PaymentsAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credencials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Fact]
        public async Task GetPaymentById_WhenPaymentExists_ReturnsOk()
        {
            // Arrange
            var cliente = _factory.CreateClient();
            var paymentId = Guid.NewGuid();

            // Act
            var response = await cliente.GetAsync($"/api/payments/{paymentId}");

            // Assert
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var body = await response.Content.ReadAsStringAsync();
                Assert.Fail($"API retornou 500. Body:{body}");
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
