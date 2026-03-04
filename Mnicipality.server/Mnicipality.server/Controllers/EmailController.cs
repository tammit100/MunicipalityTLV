using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mnicipality.server.Models;

namespace Mnicipality.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        // static parameter that saves the last response for 429 error
        public static EmailResponse? LastValidResponse { get; private set; }

        [HttpPost]
        // Enabling the restriction policy we defined in Program.cs
        [EnableRateLimiting("ThreeSecondPolicy")]
        public IActionResult PostEmail([FromBody] EmailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Invalid email");
            }

            // Create a response object with server's current time
            LastValidResponse = new EmailResponse
            {
                Email = request.Email,
                ReceivedAt = DateTime.Now
            };

            return Ok(LastValidResponse);
        }
    }
    
}

