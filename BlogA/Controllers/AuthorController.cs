using BlogA.ModelViews;
using BlogA.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly AuthorService _authorService;

        public AuthorController(AuthorService authorService)
        {
            _authorService = authorService;
        }

        // GET: api/<AuthorController>
        [HttpGet]
        public async Task<ActionResult<General>> Get()
        {
            try
            {
                var authorInfo = await _authorService.GetAuthorInfo();
                return Ok(authorInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while author info: {ex}.");

                return StatusCode(500, new { message = "An error occurred while retrieving the author information." });
            }

        }

        [HttpPost]
        public async Task<IActionResult> Post(General general)
        {
            try
            {
                await _authorService.UpdateAuthorInfo(general);

                return Ok(new { Message = "The author's profile was updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while posting/updating author info: {ex}.");
                return StatusCode(500, new { message = "An error occurred while posting/updating the author profile." });
            }

        }


    }
}
