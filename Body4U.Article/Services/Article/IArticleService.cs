﻿namespace Body4U.Article.Services.Article
{
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Common;
    using System.Threading.Tasks;

    public interface IArticleService
    {
        Task<Result<int>> Create(CreateArticleRequestModel request);

        Task<Result> Edit(EditArticleRequestModel request);
    }
}