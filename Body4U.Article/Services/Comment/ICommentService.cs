﻿namespace Body4U.Article.Services.Comment
{
    using Body4U.Common;
    using Body4U.Common.Models.Comment.Requests;
    using System.Threading.Tasks;

    public interface ICommentService
    {
        Task<Result> Create(CreateCommentRequestModel request);
    }
}