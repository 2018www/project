using BlogA.Models;
using BlogA.ModelViews;
using BlogA.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BlogA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController : ControllerBase
    {
        private readonly ChapterService _chapterService;

        public ChapterController(ChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        // GET: api/<ChapterController>
        [HttpGet]
        public async Task<ActionResult<List<ChapterView>>> GetChapters()
        {
            try
            {
                List<ChapterView> chapterViewsList = await _chapterService.GetAllChapters();
                return Ok(chapterViewsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving all chapters: {ex}.");

                return StatusCode(500, new { message = $"An error occurred while retrieving all chapters." });
            }

        }

        // GET: api/<ChapterController>
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<List<ChapterView>>> GetChaptersByBookId(int bookId)
        {
            try
            {
                List<ChapterView> chapterViewsList = await _chapterService.GetOneBookChapters(bookId);
                return Ok(chapterViewsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving all chapters: {ex}.");

                return StatusCode(500, new { message = $"An error occurred while retrieving all chapters for book({bookId})." });
            }

        }

        // GET api/<ChapterController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterView>> GetChapterById(int id)
        {
            try
            {
                var chapter = await _chapterService.GetChapterDetailById(id);
                if (chapter == null)
                {
                    return BadRequest(new { message = "Chapter not found for the provided ID." });
                }
                return Ok(chapter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving the chapter({id}): {ex}.");
                return StatusCode(500, new { message = "An error occurred while retrieving the chapter you requested." });
            }
        }

        [HttpGet("top")]
        public async Task<ActionResult<List<ChapterView>>> GetTopFiveChapters()
        {
            try
            {
                var chapterList = await _chapterService.GetTopFiveChapters();
                return Ok(chapterList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving the recent chapter(s): {ex}.");
                return StatusCode(500, new { message = "An error occurred while retrieving the recent chapter(s) you requested." });
            }
        }

        [HttpPut("top")]
        public async Task<ActionResult> UpdateTopFiveChapters(List<int> chIds)
        {
            try
            {
                if (chIds.Count > 5)
                {
                    return BadRequest(new { message = "Chapter ids exceed quantity limitaiton." });

                }
                await _chapterService.UpdateTopFiveChapters(chIds);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while updating the recent chapter(s): {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating the recent chapter(s)." });
            }
        }

        // POST api/<ChapterController>
        [HttpPost]
        public async Task<IActionResult> Post(ChapterView newChapter)
        {
            try
            {
                newChapter.ChapterId = await _chapterService.CreateChapter(newChapter);

                return CreatedAtAction(nameof(GetChapterById), new { id = newChapter.ChapterId }, newChapter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while posting the chapter: {ex}.");
                return StatusCode(500, new { message = "An error occurred while posting the chapter." });
            }

        }

        // PUT api/<ChapterController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ChapterView updatedChapter)
        {
            try
            {
                var chapter = await _chapterService.GetChapterDetailById(id);
                if (chapter is null)
                {
                    return NotFound(new { Message = $"Update failed. Chapter({id}) is not in the system." });
                }
                await _chapterService.UpdateChapter(updatedChapter);
                return Ok(new { Message = "The chapter was updated successfully." });
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error occurred while updating the chapter: {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating the chapter." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    return BadRequest("No IDs provided.");
                }

                List<int> idList = ids.Split(',').Select(int.Parse).ToList();

                if (idList.Count <=1)
                {
                    return BadRequest("No chapter IDs provided.");
                }

                int bookId = idList[0];
                idList.RemoveAt(0);
                List<int> chapterIds = idList;

                await _chapterService.DeleteOneBookMultipleChapterByIds(bookId, chapterIds);

                return NoContent();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deleting the chapter(s): {ex}.");
                return StatusCode(500, new { message = "An error occurred while deleting the chapter(s)." });
            }

        }

       

        [HttpPut("book/{bookId}/seq")]
        public async Task<IActionResult> UpdateOneBookChapterSequence(int bookId, Dictionary<int, List<int>> newSecChSeq)
        {
            try
            {
                 await _chapterService.UpdateOneBookChapterSeq(bookId,newSecChSeq);
                return Ok(new { Message = $"The book chapter sequence was updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while updating book({bookId}) chapter seq: {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating book chapter sequence." });
            }

        }

        [HttpPut("book/{bookId}/newest")]
        public async Task<IActionResult> UpdateOneBookNewestChapter(int bookId, int chapterId)
        {
            try
            {
                //pass only bookId means remove newest chapter
                //pass bookId and chapterId means set this chapter as this book's newest
                List<int> ids = new() { bookId };
                if (chapterId !=0)
                {
                    ids.Add(chapterId);
                }

                await _chapterService.SetNewestChapter(ids);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while updating book({bookId}) newest chapter: {ex}.");
                return StatusCode(500, new { message = "An error occurred while  updating book newest chapter." });
            }

        }


    }
}
