namespace Body4U.Article.Services.Comment
{
    using Body4U.Article.Data;
    using Body4U.Article.Data.Models;
    using Body4U.Article.Models.Requests.Comment;
    using Body4U.Common;
    using Body4U.Common.Models.Comment.Requests;
    using Body4U.Common.Services.Identity;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.Article;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Comment;
    using System.Linq;

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

        public async Task<Result> Edit(EditCommentRequestModel request)
        {
            try
            {
                var comment = await this.dbContext
                    .Comments
                    .FindAsync(new object[] { request.Id });

                if (comment == null)
                {
                    return Result.Failure(CommentMissing);
                }

                if (comment.ApplicationUserId != request.UserId && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                comment.Content = request.Content;

                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(CommentService)}.{nameof(Edit)}");
                return Result.Failure(string.Format(Wrong, nameof(Edit)));
            }
        }

        public async Task<Result> Delete(DeleteCommentRequestModel request)
        {
            try
            {
                var comment = await this.dbContext
                    .Comments
                    .FindAsync(new object[] { request.Id });

                if (comment == null)
                {
                    return Result.Failure(CommentMissing);
                }

                if (comment.ApplicationUserId != request.UserId && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                this.dbContext.Comments.Remove(comment);
                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(CommentService)}.{nameof(Delete)}");
                return Result.Failure(string.Format(Wrong, nameof(Delete)));
            }
        }

        public async Task<Result<List<SearchCommentsResponseModel>>> Search(SearchCommentsRequestModel request)
        {
            try
            {
                var articleExists = await this.dbContext
                    .Articles
                    .AnyAsync(x => x.Id == request.ArticleId);

                if (!articleExists)
                {
                    return Result<List<SearchCommentsResponseModel>>.Failure(ArticleMissing);
                }

                var pageIndex = request.PageIndex;
                var pageSize = request.PageSize;

                var comments = await this.dbContext
                    .Comments
                    .Where(x => x.ArticleId == request.ArticleId)
                    .Select(x=> new SearchCommentsResponseModel()
                    {
                        Id = x.Id,
                        Content = x.Content,
                        ApplicationUserId = x.ApplicationUserId,
                        DatePosted = x.CreatedOn
                    })
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Result<List<SearchCommentsResponseModel>>.SuccessWith(comments);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(CommentService)}.{nameof(Search)}");
                return Result<List<SearchCommentsResponseModel>>.Failure(string.Format(Wrong, nameof(Search)));
            }
        }
    }
}
