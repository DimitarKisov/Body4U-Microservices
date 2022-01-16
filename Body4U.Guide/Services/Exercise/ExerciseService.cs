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
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.Exercise;

    public class ExerciseService : IExerciseService
    {
        private readonly GuideDbContext dbContext;

        public ExerciseService(GuideDbContext dbContext)
            => this.dbContext = dbContext;

        public async Task<Result<CreateExerciseResponseModel>> Create(CreateExerciseRequestModel request)
        {
            try
            {
                var nameExists = await this.dbContext
                    .Exercises
                    .AnyAsync(x => x.Name == request.Name);

                if (nameExists)
                {
                    return Result<CreateExerciseResponseModel>.Failure(NameTaken);
                }

                if (!Enum.IsDefined(typeof(ExerciseType), request.ExerciseType))
                {
                    return Result<CreateExerciseResponseModel>.Failure(WrongExerciseType);
                }

                var exercise = new Exercise()
                {
                    Name = request.Name,
                    Description = request.Description,
                    ExerciseType = (ExerciseType)request.ExerciseType,
                    ExerciseDifficulty = (ExerciseDifficulty)request.ExerciseDifficulty
                };

                await this.dbContext.AddAsync(exercise);
                await this.dbContext.SaveChangesAsync();

                return Result<CreateExerciseResponseModel>.SuccessWith(new CreateExerciseResponseModel { Id = exercise.Id });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}.{nameof(Create)}");
                return Result<CreateExerciseResponseModel>.Failure(string.Format(Wrong, nameof(Create)));
            }
        }

        public async Task<Result<GetExerciceResponseModel>> Get(int id)
        {
            try
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
                    return Result<GetExerciceResponseModel>.Failure(ExerciseMissing);
                }

                return Result<GetExerciceResponseModel>.SuccessWith(exercise);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}.{nameof(Get)}");
                return Result<GetExerciceResponseModel>.Failure(string.Format(Wrong, nameof(Get)));
            }
        }

        public async Task<Result<SearchExercisesResponseModel>> Search(SearchExercisesRequestModel request)
        {
            try
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

                var orderBy = "Id";

                if (!string.IsNullOrWhiteSpace(sortingField))
                {
                    if (sortingField.ToLower() == "name")
                    {
                        orderBy = nameof(request.Name);
                    }
                    else if (sortingField.ToLower() == "exercisedifficulty")
                    {
                        orderBy = nameof(request.ExerciseDifficulty);
                    }
                }

                if (sortingOrder != null && sortingOrder.ToLower() == Desc)
                {
                    exercises = exercises.OrderByDescending(x => orderBy);
                }
                else
                {
                    exercises = exercises.OrderBy(x => orderBy);
                }

                var data = await exercises
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Result<SearchExercisesResponseModel>.SuccessWith(new SearchExercisesResponseModel() { Data = data, TotalRecords = totalRecords });

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}.{nameof(Search)}");
                return Result<SearchExercisesResponseModel>.Failure(string.Format(Wrong, nameof(Search)));
            }
        }

        public async Task<Result> Edit(EditExerciseRequestModel request)
        {
            try
            {
                var exercise = await this.dbContext
                    .Exercises
                    .FindAsync(new object[] { request.Id });

                if (exercise == null)
                {
                    return Result.Failure(ExerciseMissing);
                }

                var nameExists = await this.dbContext
                    .Exercises
                    .AnyAsync(x => x.Name == request.Name);

                if (nameExists)
                {
                    return Result.Failure(NameTaken);
                }

                if (!Enum.IsDefined(typeof(ExerciseType), request.ExerciseType))
                {
                    return Result.Failure(WrongExerciseType);
                }

                exercise.Name = request.Name;
                exercise.Description = request.Description;
                exercise.ExerciseType = (ExerciseType)request.ExerciseType;
                exercise.ExerciseDifficulty = (ExerciseDifficulty)request.ExerciseDifficulty;

                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}.{nameof(Edit)}");
                return Result.Failure(string.Format(Wrong, nameof(Edit)));
            }
        }

        public async Task<Result> Delete(int id)
        {
            try
            {
                var exercise = await this.dbContext
                    .Exercises
                    .FindAsync(new object[] { id });

                if (exercise == null)
                {
                    return Result.Failure(ExerciseMissing);
                }

                this.dbContext.Exercises.Remove(exercise);
                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}.{nameof(Delete)}");
                return Result.Failure(string.Format(Wrong, nameof(Delete)));
            }
        }
    }
}
