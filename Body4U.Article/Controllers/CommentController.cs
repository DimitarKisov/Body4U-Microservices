namespace Body4U.Article.Controllers
{
    using Body4U.Article.Services.Comment;
    using Body4U.Common.Controllers;
    using Body4U.Common.Models.Comment.Requests;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Authorize]
    public class CommentController : ApiController
    {
        private readonly ICommentService commentService;

        public CommentController(ICommentService commentService)
            => this.commentService = commentService;

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<ActionResult> Create(CreateCommentRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.commentService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.BadRequest(result.Errors);
            }

            return this.Ok();
        }
    }
}
