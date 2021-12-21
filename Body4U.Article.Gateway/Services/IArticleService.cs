﻿namespace Body4U.Article.Gateway.Services
{
    using Body4U.Common.Models.Article.Responses;
    using Refit;
    using System.Threading.Tasks;

    public interface IArticleService
    {
        [Get("/Article/{id}")]
        Task<GetArticleResponseModel> Get(int id);
    }
}