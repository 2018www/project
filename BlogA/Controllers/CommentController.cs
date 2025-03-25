using BlogA.ModelViews;
using BlogA.Services;
using Microsoft.AspNetCore.Mvc;


namespace BlogA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {

        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/<CommentController>
        [HttpGet]
        public async Task<ActionResult<List<CommentView>>> Get()
        {
            try
            {
                var commentList = await _commentService.GetAllComments();
                return Ok(commentList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving all comments: {ex}.");

                return StatusCode(500, new { message = "An error occurred while retrieving all comment." });
            }

        }

        // GET api/<CommentController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentView>> GetCommentById(int id)
        {
            try
            {
               var comment =  await _commentService.GetCommentById(id);
                if (comment == null) { 
                    return BadRequest(new { message = "Comment not found for the provided ID." });
                }
                return Ok(comment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving comment({id}): {ex}.");

                return StatusCode(500, new { message = "An error occurred while retrieving the comment." });
            }
        }

        [HttpGet("book/{id}/comment")]
        public async Task<ActionResult<CommentView>> GetCommentsByBookId(int id)
        {
            try
            {
                var commentList = await _commentService.GetCommentByBookId(id);
                return Ok(commentList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving book({id}) commentList: {ex}.");

                return StatusCode(500, new { message = "An error occurred while retrieving the comments related to the requested book." });
            }
        }

        // POST api/<CommentController>
        [HttpPost]
        public async Task<IActionResult> Post(CommentView commentView)
        {
            try
            {
                if (commentView == null || string.IsNullOrWhiteSpace(commentView.Username) )
                {
                    return BadRequest(new { message = "Post comment failed. Please provide valid data." });
                }
                int commentId = await _commentService.CreateComment(commentView);
                commentView.CommentId = commentId;
                return CreatedAtAction(nameof(GetCommentById), new { id = commentId }, commentView);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while posting comment: {ex}.");
                return StatusCode(500, new { message = "An error occurred while posting the comment." });
            }
        }

        // PUT api/<CommentController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(CommentView commentView)
        {
            try
            {
                var cmt = await _commentService.GetCommentById(commentView.CommentId);
                if(cmt == null)
                {
                    return NotFound(new { Message = $"Update failed. Comment ({commentView.CommentId}) is not in the system." });
                }
                await _commentService.UpdateComment(commentView);
                return Ok(new { Message = "The comment was updated successfully." });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while updating comment: {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating the comment." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMultiple(List<CommentView> commentViews)
        {
            try
            {
                if (!commentViews.Any())
                {
                    return BadRequest(new { message = "No IDs provided." });
                }

                await _commentService.UpdateMultipleComments(commentViews);
                return NoContent();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while udpating comment(s): {ex}.");
                return StatusCode(500, new { message = "An error occurred while updating the comment(s)." });
            }
        }



        // DELETE api/<CommentController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Comment not found for the provided ID." });
                }

                var cmt = await _commentService.GetCommentById(id);
                if (cmt == null)
                {
                    return NotFound(new { Message = $"Delete failed. Comment ({id}) is not in the system." });
                }
                await _commentService.DeleteCommentById(id);
                return NoContent();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deleting comment: {ex}.");
                return StatusCode(500, new { message = "An error occurred while deleting the comment." });
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
              
                if (!idList.Any())
                {
                    return BadRequest(new { message = "No IDs provided." });
                }
                if (idList.Any(x=>x<=0))
                {
                    return BadRequest(new { message = "One ore more invalid ID is in the provided IDs." });
                }
                await _commentService.DeleteByMultipleCommentIds(idList);
                return NoContent();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deleting comment(s): {ex}.");
                return StatusCode(500, new { message = "An error occurred while deleting the comment(s)." });
            }
        }

    }
}
