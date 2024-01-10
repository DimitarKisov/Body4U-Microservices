namespace Body4U.Identity.Services.Favourites
{
    using Body4U.Common;
    using Body4U.Common.Models.Favourites.Requests;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Models.Favourites;
    using Body4U.Identity.Data.Models.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.ApplicationUser;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

    public class FavouritesService : IFavouritesService
    {
        private readonly IdentityDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICurrentUserService currentUserService;

        public FavouritesService(
            IdentityDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.currentUserService = currentUserService;
        }

        public async Task<Result> Add(AddToFavouritesRequestModel request)
        {
            var user = await this.userManager
                    .FindByIdAsync(request.ApplicationUserId);

            if (user == null)
            {
                return Result.Failure(NotFound, string.Format(UserNotFound, request.ApplicationUserId));
            }

            var alreadyInFavourites = await this.dbContext
                .Favourites
                .AnyAsync(x => x.ArticleId == request.ArticleId && x.ApplicationUserId == user.Id);

            if (alreadyInFavourites)
            {
                return Result.Failure(Conflict, AlreadyInFavourites);
            }

            var favouriter = new Favourite()
            {
                ApplicationUserId = user.Id,
                ArticleId = request.ArticleId,
                AddedIn = DateTime.Now
            };

            user.Favourites.Add(favouriter);
            try
            {
                this.dbContext.Favourites.Add(favouriter);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format(Wrong, $"{nameof(FavouritesService)}/{nameof(Add)}"));
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }

        public async Task<Result> Remove(RemoveFromFavouritesRequestModel request)
        {
            var user = await this.userManager
                       .FindByIdAsync(request.ApplicationUserId);

            if (user == null)
            {
                return Result.Failure(NotFound, string.Format(UserNotFound, request.ApplicationUserId));
            }

            var favourite = await this.dbContext
                .Favourites
                .FirstOrDefaultAsync(x => x.ArticleId == request.ArticleId && x.ApplicationUserId == user.Id);

            if (favourite == null)
            {
                return Result.Failure(NotFound, NotInFavourites);
            }

            user.Favourites.Remove(favourite);

            try
            {
                this.dbContext.Favourites.Remove(favourite);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format(Wrong, $"{nameof(FavouritesService)}/{nameof(Remove)}"));
                return Result.Failure(InternalServerError, UnhandledError);
            }

            return Result.Success;
        }

        public async Task<Result<List<int>>> Mines()
        {
            var favourites = await this.dbContext
                .Favourites
                .Where(x => x.ApplicationUserId == this.currentUserService.UserId)
                .OrderByDescending(x => x.AddedIn)
                .Select(x => x.ArticleId)
                .ToListAsync();

            return Result<List<int>>.SuccessWith(favourites);
        }
    }
}
