using ContentAggregator.Core.Models;
using ContentAggregator.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContentAggregator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FbPostsController : ControllerBase
    {
        private readonly FbPoster _fbPoster;

        public FbPostsController(FbPoster fbPoster)
        {
            _fbPoster = fbPoster;
        }

        // POST: api/FbPosts
        [HttpPost]
        public async Task<IActionResult> SharePost(Post post)
        {
            try
            {
                string result = await _fbPoster.SharePost(post.PageId, post.Url?.ToString(), post.CustomText);
                return Ok(new { Result = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
