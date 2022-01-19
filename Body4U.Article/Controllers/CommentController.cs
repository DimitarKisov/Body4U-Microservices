namespace Body4U.Article.Controllers
{
    using Body4U.Article.Models.Requests.Comment;
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
                return this.ProcessErrors(result);
            }

            return this.Created(string.Empty, null); //TODO: Така ли ще бъде, тъй като ние добавяме коментар в статия, в която вече сме влезли? или да е NoContent?
        }

        [HttpPut]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit(EditCommentRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.commentService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<ActionResult> Delete(DeleteCommentRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.commentService.Delete(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(nameof(Search))]
        public async Task<ActionResult> Search(SearchCommentsRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.commentService.Search(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }
    }
}
