namespace Body4U.Article.Services.Comment
{
    using Body4U.Article.Data;
    using Body4U.Common;
    using Body4U.Common.Models.Comment.Requests;
    using Body4U.Common.Services.Identity;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using Serilog;
    using System;

    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Article;
    
    public class CommentService : ICommentService
    {
        private readonly ArticleDbContext dbContext;
        private readonly ICurrentUserService currentUserService;

        public CommentService(
            ArticleDbContext dbContext,
            ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
        }

        public async Task<Result> Create(CreateCommentRequestModel request)
        {
            try
            {
                var articleExists = await this.dbContext
                .Articles
                .AnyAsync(x => x.Id == request.ArticleId);

                if (!articleExists)
                {
                    return Result.Failure(ArticleMissing);
                }

                var comment = new Comment()
                {
                    Content = request.Content,
                    CreatedOn = DateTime.Now,
                    ArticleId = request.ArticleId,
                    ApplicationUserId = this.currentUserService.UserId
                };

                await this.dbContext.Comments.AddAsync(comment);
                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(CommentService)}.{nameof(Create)}");
                return Result.Failure(string.Format(Wrong, nameof(Create)));
            }
        }
    }
}
