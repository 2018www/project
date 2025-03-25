
using Microsoft.AspNetCore.Mvc;

namespace BlogA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello, World");
        }
    }
}
