using Microsoft.AspNetCore.Mvc;
using BlogA.Services;
using BlogA.ModelViews;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BlogA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;

        public BookController(BookService bookService)
        {
            _bookService = bookService;
        }

        // GET: api/<BookController>
        [HttpGet]
        public async Task<ActionResult<List<BookView>>> Get()
        {
            try
            {
                var bookList =  await _bookService.GetAllBook();
                return Ok(bookList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving all books: {ex}.");

                return StatusCode(500, new { message = "An error occurred while retrieving all books." });
            }

        }

        // GET api/<BookController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookView>> Get(int id)
        {

            try
            {
                var book = await _bookService.GetBookById(id);
                if (book == null) {
                    return BadRequest(new { message = "Book not found for the provided ID." });
                }
                return Ok(book);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving the book({id}): {ex}.");
                return StatusCode(500, new { message = "An error occurred while retrieving the book you requested." });
            }


        }

        // POST api/<BookController>
        [HttpPost]
        public async Task<IActionResult> Post(BookView newBook)
        {
            try
            {
                newBook.BookId =  await _bookService.CreateBook(newBook);

                return CreatedAtAction(nameof(Get), new { id = newBook.BookId }, newBook);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while posting the book: {ex}.");
                return StatusCode(500, new { message = "An error occurred while posting the book." });
            }

        }

        // PUT api/<BookController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBasic(int id, BookView updatedBook)
        {
            try
            {
                var book = await _bookService.GetBookById(id);
                if (book is null)
                {
                    return NotFound(new { Message = $"Update failed. Book ({id}) is not in the system." });
                }
                await _bookService.UpdateBookBasic(book.BookId, updatedBook);
                return Ok(new { Message = "The book was updated successfully." });
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error occurred while updating the book: {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating the book." });
            }
        }

        [HttpPut("{id}/sec")]
        public async Task<IActionResult> UpdateSection(int id, BookView updatedBook)
        {
            try
            {
                var book = await _bookService.GetBookById(id);
                if (book is null)
                {
                    return NotFound(new { Message = $"Update failed. Book ({id}) is not in the system." });
                }
                await _bookService.UpdateBookSectionList(book.BookId, updatedBook);
                return Ok(new { Message = "The book was updated successfully." });
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error occurred while updating the book: {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating the book." });
            }
        }

        [HttpPut("{id}/char")]
        public async Task<IActionResult> UpdateCharacter(int id, BookView updatedBook)
        {
            try
            {
                var book = await _bookService.GetBookById(id);
                if (book is null)
                {
                    return NotFound(new { Message = $"Update failed. Book ({id}) is not in the system." });
                }
                await _bookService.UpdateBookCharacterList(book.BookId, updatedBook);
                return Ok(new { Message = "The book was updated successfully." });
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error occurred while updating the book: {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating the book." });
            }
        }

        // DELETE api/<BookController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var book = await _bookService.GetBookById(id);

                if (book is null)
                {
                    return NotFound(new { Message = $"Delete failed. Book ({id}) is not in the system." });
                }

                await _bookService.DeleteBook(id);

                return NoContent();

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error occurred while deleting the book: {ex}.");
                return StatusCode(500, new { message = "An error occurred while deleting the book." });
            }

        }

        [HttpGet("seq")]
        public async Task<IActionResult> GetBookSequence()
        {
            try
            {
                List<int> seq = new();
                seq = await _bookService.GetBookSeqence();
                return Ok(seq);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while getting book seq: {ex}.");
                return StatusCode(500, new { message = "An error occurred while  getting the sequence of all books." });
            }

        }

        [HttpPut("seq")]
        public async Task<IActionResult> UpdateBookSequence(List<int> newSeq)
        {
            try
            {
                await _bookService.UpdateBookSeqence(newSeq);
                return Ok(new { Message = $"The book sequence was updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while updating book seq: {ex}.");
                return StatusCode(500, new { message = "An error occurred while  updating the sequence of all books." });
            }

        }
    }
}
