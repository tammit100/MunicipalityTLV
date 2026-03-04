using Microsoft.AspNetCore.RateLimiting;
using Mnicipality.server.Controllers;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- Rate Limiting Service ---
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ThreeSecondPolicy", opt =>
    {
        opt.PermitLimit = 1;
        opt.Window = TimeSpan.FromSeconds(3);
        //opt.QueueLimit = 0;
    });

    #region restrict requests by IP
    /// <summary>
    /// If restrict requests by IP required
    /// </summary>
    /// <param>No parameters requierd (_)</param>
    /// <returns></returns>
    //options.AddPolicy("ThreeSecondPolicy", context =>
    //{
    //    var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Global";

    //    return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
    //    {
    //        PermitLimit = 1,
    //        Window = TimeSpan.FromSeconds(3),
    //        QueueLimit = 0,
    //        //AutoReplenishment = true
    //    });
    //});
    #endregion

    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        // last response from controller
        if (EmailController.LastValidResponse != null)
        {
            await context.HttpContext.Response.WriteAsJsonAsync(EmailController.LastValidResponse);
        }
    };
});


#region restrict requests by Email
/// <summary>
/// If restrict requests by IP required
/// </summary>
/// <param>No parameters requierd (_)</param>
/// <returns></returns>
//builder.Services.AddRateLimiter(options =>
//{
//    // Policy definition
//    options.AddPolicy("ThreeSecondPolicy", context =>
//    {
//        return RateLimitPartition.GetFixedWindowLimiter(
//            partitionKey: GetEmailFromRequest(context), // getting the Email
//            factory: _ => new FixedWindowRateLimiterOptions
//            {
//                PermitLimit = 1,
//                Window = TimeSpan.FromSeconds(3),
//                QueueLimit = 0,
//                // AutoReplenishment = true
//            });
//    });

//    options.OnRejected = async (context, _) =>
//    {
//        context.HttpContext.Response.StatusCode = 429;
//        if (EmailController.LastValidResponse != null)
//        {
//            await context.HttpContext.Response.WriteAsJsonAsync(EmailController.LastValidResponse);
//        }
//    };
//});

//// reading the email (async)
//string GetEmailFromRequest(HttpContext context)
//{
//    context.Request.EnableBuffering();
//    try
//    {
//        using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
//        {
//            // קריאה סינכרונית - בתוך Middleware זה תקין אם ה-Stream ב-Buffer
//            var body = reader.ReadToEnd();
//            context.Request.Body.Position = 0;

//            if (!string.IsNullOrEmpty(body))
//            {
//                using var jsonDoc = JsonDocument.Parse(body);
//                if (jsonDoc.RootElement.TryGetProperty("email", out var emailProp))
//                {
//                    return emailProp.GetString() ?? "anonymous";
//                }
//            }
//        }
//    }
//    catch { }
//    return "anonymous";
//}
#endregion


// Add CORS so Angular can talk to the API
builder.Services.AddCors(options =>
{
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
