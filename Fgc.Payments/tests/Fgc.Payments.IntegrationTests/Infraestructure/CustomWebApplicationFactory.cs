using Fgc.Payments.Api.Consumers;
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
            builder.ConfigureTestServices(services =>
            {
                // 1. Banco InMemory 
                services.RemoveAll(typeof(DbContextOptions<PaymentsDbContext>));
                services.RemoveAll(typeof(DbContextOptions));

                services.AddScoped<DbContextOptions<PaymentsDbContext>>(provider =>
                {
                    return new DbContextOptionsBuilder<PaymentsDbContext>()
                        .UseInMemoryDatabase("InMemoryPaymentTestDb")
                        .Options;
                });

                // 2. MassTransit InMemory 
                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<OrderPlacedEventConsumer>();
                });
            });
        }
    }
}