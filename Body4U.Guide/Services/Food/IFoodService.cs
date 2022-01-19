﻿namespace Body4U.Guide.Services.Food
{
    using Body4U.Common;
    using Body4U.Guide.Models.Requests.Food;
    using System.Threading.Tasks;

    public interface IFoodService
    {
        Task<Result<int>> Create(CreateFoodRequestModel request);
    }
}