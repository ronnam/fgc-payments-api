using Fgc.Payments.Application.Consumers;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Application.Services;
using Fgc.Payments.Infraestructure.Persistence;
using Fgc.Payments.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static MassTransit.Logging.DiagnosticHeaders.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// 1. CONFIGURAÇÕES DO MICROSSERVIÇO DE PAGAMENTOS
// ============================================================

// A. Banco de Dados (Entity Framework Core)
builder.Services.AddDbContext<FgcPaymentsDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("PaymentsDb")));

// B. Injeção de Dependências (Repositories)
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// C. Injeção de Dependências (Use Cases / Services)
builder.Services.AddScoped<IProcessPaymentUseCase, ProcessPaymentUseCase>();

// D. Autenticação JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearere", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// E.MassTransit / RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registra o consumer
    x.AddConsumer<OrderPlacedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username("admin");
            h.Password("admin");
        });

        // Nome da fila que o Payments vai escutar
        cfg.ReceiveEndpoint("payments-order-placed-queue", e =>
        {
            e.ConfigureConsumer<OrderPlacedEventConsumer>(context);
        });
    });
});

// ============================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ============================================================
// 2. MIDDLEWARES DE SEGURANÇA
// ============================================================
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using(var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FgcPaymentsDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

public partial class Program { } // Para testes de integração
