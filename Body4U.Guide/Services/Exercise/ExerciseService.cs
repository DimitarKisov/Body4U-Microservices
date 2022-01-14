﻿namespace Body4U.Guide.Services.Exercise
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
                    ExerciseType = (ExerciseType)request.ExerciseType
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
                        ExerciseType = (int)x.ExerciseType
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

                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ExerciseService)}.{nameof(Edit)}");
                return Result.Failure(string.Format(Wrong, nameof(Edit)));
            }
        }
    }
}
