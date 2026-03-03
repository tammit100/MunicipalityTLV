using Microsoft.AspNetCore.RateLimiting;
using Mnicipality.server.Controllers;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- Rate Limiting Service ---
builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("ThreeSecondPolicy", opt => {
        opt.PermitLimit = 1;
        opt.Window = TimeSpan.FromSeconds(3);
    });

    options.OnRejected = async (context, _) => {
        context.HttpContext.Response.StatusCode = 429;
        // last response from controller
        if (EmailController.LastValidResponse != null)
        {
            await context.HttpContext.Response.WriteAsJsonAsync(EmailController.LastValidResponse);
        }
    };
});

// Add CORS so Angular can talk to the API
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- 2. Enable the Middlewares ---
app.UseCors();
app.UseRateLimiter();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
