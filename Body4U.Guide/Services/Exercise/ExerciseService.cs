namespace Body4U.Guide.Services.Exercise
{
    using Body4U.Common;
    using Body4U.Guide.Data;
    using Body4U.Guide.Data.Models;
    using Body4U.Guide.Models.Requests.Exercise;
    using Body4U.Guide.Models.Responses.Exercise;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Exercise;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

    public class ExerciseService : IExerciseService
    {
        private readonly GuideDbContext dbContext;

        public ExerciseService(GuideDbContext dbContext)
            => this.dbContext = dbContext;

        public async Task<Result<CreateExerciseResponseModel>> Create(CreateExerciseRequestModel request)
        {
            if (!Enum.IsDefined(typeof(ExerciseType), request.ExerciseType))
            {
                return Result<CreateExerciseResponseModel>.Failure(BadRequest, WrongExerciseType);
            }

            var nameExists = await this.dbContext
                    .Exercises
                    .AnyAsync(x => x.Name == request.Name);

            if (nameExists)
            {
                return Result<CreateExerciseResponseModel>.Failure(Conflict, NameTaken);
            }

            var exercise = new Exercise()
            {
                Name = request.Name,
                Description = request.Description,
                ExerciseType = (ExerciseType)request.ExerciseType,
                ExerciseDifficulty = (ExerciseDifficulty)request.ExerciseDifficulty
            };

            try
            {
                await this.dbContext.AddAsync(exercise);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}/{nameof(Create)}");
                return Result<CreateExerciseResponseModel>.Failure(InternalServerError, UnhandledError);
            }

            return Result<CreateExerciseResponseModel>.SuccessWith(new CreateExerciseResponseModel { Id = exercise.Id });
        }

        public async Task<Result<GetExerciceResponseModel>> Get(int id)
        {
            var exercise = await this.dbContext
                    .Exercises
                    .Select(x => new GetExerciceResponseModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        ExerciseType = (int)x.ExerciseType,
                        ExerciseDifficulty = (int)x.ExerciseDifficulty
                    })
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (exercise == null)
            {
                return Result<GetExerciceResponseModel>.Failure(NotFound, ExerciseMissing);
            }

            return Result<GetExerciceResponseModel>.SuccessWith(exercise);
        }

        public async Task<Result<SearchExercisesResponseModel>> Search(SearchExercisesRequestModel request)
        {
            var exercises = this.dbContext
                    .Exercises
                    .Select(x => new ExerciseResponseModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExerciseDifficulty = (int)x.ExerciseDifficulty
                    })
                    .AsQueryable();

            var totalRecords = await exercises.CountAsync();

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;
            var sortingOrder = request.OrderBy!;
            var sortingField = request.SortBy!;

            Expression<Func<ExerciseResponseModel, object>> sortingExpression = x => x.Id;

            if (!string.IsNullOrWhiteSpace(sortingField))
            {
                if (sortingField.ToLower() == "name")
                {
                    sortingExpression = x => x.Name;
                }
                else if (sortingField.ToLower() == "exercisedifficulty")
                {
                    sortingExpression = x => x.ExerciseDifficulty;
                }
            }

            if (sortingOrder != null && sortingOrder.ToLower() == Desc)
            {
                exercises = exercises.OrderByDescending(sortingExpression);
            }
            else
            {
                exercises = exercises.OrderBy(sortingExpression);
            }

            var data = await exercises
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result<SearchExercisesResponseModel>.SuccessWith(new SearchExercisesResponseModel() { Data = data, TotalRecords = totalRecords });
        }

        public async Task<Result> Edit(EditExerciseRequestModel request)
        {
            if (!Enum.IsDefined(typeof(ExerciseType), request.ExerciseType))
            {
                return Result.Failure(BadRequest, WrongExerciseType);
            }

            var nameExists = await this.dbContext
                .Exercises
                .AnyAsync(x => x.Name == request.Name);

            if (nameExists)
            {
                return Result.Failure(Conflict, NameTaken);
            }

            var exercise = await this.dbContext
                    .Exercises
                    .FindAsync(new object[] { request.Id });

            if (exercise == null)
            {
                return Result.Failure(NotFound, ExerciseMissing);
            }

            exercise.Name = request.Name;
            exercise.Description = request.Description;
            exercise.ExerciseType = (ExerciseType)request.ExerciseType;
            exercise.ExerciseDifficulty = (ExerciseDifficulty)request.ExerciseDifficulty;

            await this.dbContext.SaveChangesAsync();

            return Result.Success;
        }

        public async Task<Result> Delete(int id)
        {
            var exercise = await this.dbContext
                    .Exercises
                    .FindAsync(new object[] { id });

            if (exercise == null)
            {
                return Result.Failure(NotFound, ExerciseMissing);
            }

            try
            {
                this.dbContext.Exercises.Remove(exercise);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}/{nameof(Delete)}");
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }

        public async Task<Result<Dictionary<int, string>>> AutocompleteExerciseName(string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                var articlesTitles = await this.dbContext.Exercises
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .Where(x => x.Name.ToLower().Contains(term.ToLower()))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

                return Result<Dictionary<int, string>>.SuccessWith(articlesTitles);
            }

            return Result<Dictionary<int, string>>.SuccessWith(new Dictionary<int, string>());
        }
    }
}
