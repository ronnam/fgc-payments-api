using Fgc.Payments.Application.Consumers;
using Fgc.Payments.Infraestructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fgc.Payments.IntegrationTests.Infraestructure
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Define o ambiente como "Testing" - O Program ignorará o RabbitMQ
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<FgcPaymentsDbContext>));
                services.RemoveAll(typeof(DbContextOptions));

                services.AddScoped<DbContextOptions<FgcPaymentsDbContext>>(provider =>
                {
                    return new DbContextOptionsBuilder<FgcPaymentsDbContext>()
                        .UseInMemoryDatabase("InMemoryPaymentTestDb")
                        .Options;
                });

                // MassTransit in-memory com TestHarness
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<OrderPlacedEventConsumer>();
                });
            });
        }
    }
}
