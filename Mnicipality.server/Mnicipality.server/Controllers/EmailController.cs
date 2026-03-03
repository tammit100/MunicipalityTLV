using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Mnicipality.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        // משתנה סטטי ששומר את התגובה המוצלחת האחרונה עבור שגיאת 429
        public static EmailResponse? LastValidResponse { get; private set; }

        [HttpPost]
        // הפעלת מדיניות ההגבלה שהגדרנו ב-Program.cs
        [EnableRateLimiting("ThreeSecondPolicy")]
        public IActionResult PostEmail([FromBody] EmailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Invalid email");
            }

            // יצירת אובייקט תגובה עם הזמן הנוכחי בשרת
            LastValidResponse = new EmailResponse
            {
                Email = request.Email,
                ReceivedAt = DateTime.Now
            };

            return Ok(LastValidResponse);
        }
    }

    // מודלים (Dating Objects)
    public class EmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class EmailResponse
    {
        public string Email { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
    }
}

