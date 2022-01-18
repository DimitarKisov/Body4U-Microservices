using System.Net;

namespace Body4U.Common.Constants
{
    public class MessageConstants
    {
        public class Common
        {
            public const string ChangeProfilePictureDeny = "First delete the current profile picture before you upload a new one.";
            public const string EmailConfirmHtmlContent = "<p>To confirm your email, please click <a href=\"{0}\">HERE</a></p>";
            public const string EmailConfirmSubject = "Email Confirmation";
            public const string EmptyFile = "Empty file.";
            public const string EmailProblem = "Email was not sent successfuly.";
            public const string ForgotPasswordHtmlContent = "<p>To reset your password, please click <a href=\"{0}\">HERE</a></p>";
            public const string ForgotPasswordSubject = "Password Reset";
            public const string ImageNotFound = "Image not found.";
            public const string ServerError = "Internal server error.";
            public const string MultipleNoImagesChosen = "There isn't any images chosen.";
            public const string MultipleWrongImageFormats = "There is a unsupported format in your images.";
            public const string MultipleWrongWidthOrHeight = "There are images with width under {0} or height under {1} pixels.";
            public const string NoImage = "There is no image chosen.";
            public const string Wrong = "Something went wrong in {0}.";
            public const string WrongImageFormat = "Please choose a picture with format .jpg/.jpeg or .png";
            public const string WrongWidthOrHeight = "Please upload a image with min width: {0} and min height: {1}.";
            public const string WrongWrights = "You don't have the rights for this action!";
            public const string UnhandledError = "Unhandled Error";

            public const string UpdatingDbEntities = "Updating and saving db entities in database.";
            public const string AddingDbEntities = "Adding and saving db entities in database.";
            public const string SavingDbEntities = "Saving db entities in database.";
            public const string Publish = "publishing messages.";
        }

        public class Article
        {
            public const string ArticleMissing = "There is no such article.";
            public const string TitleTaken = "This title is already taken.";
            public const string WrongArticleType = "Wrong article type.";
        }

        public class ApplicationUser
        {
            public const string AlreadyInFavourites = "Article is already in favourites.";
            public const string EmailNotConfirmed = "Email is not confirmed.";
            public const string Locked = "Account is locked.";
            public const string LoginFailed = "Login failed.";
            public const string NotInFavourites = "Article is not in favourites.";
            public const string ProfilePictureNotFound = "No profile picture found to change.";
            public const string TrainerNotFound = "User with ID '{0}' is not a trainer.";
            public const string UserNotFound = "There is not such user with Id '{0}'";
            public const string WrongGender = "There is no such gender.";
            public const string WrongUsernameOrPassword = "Wrong email or password.";
        }

        public class Trainer
        {
            public const string MaxAllowedImages = "The max allowed images you can upload is {0}";
            public const string NotReady = "Fill in the necessary information about you.";
            public const string TooManyImages = "The max allowed images is {0}. You can upload {1} more images.";
            public const string TrainerNotFound = "User is not a trainer.";
        }

        public class Comment
        {
            public const string CommentMissing = "There is no such comment.";
        }

        public class Service
        {
            public const string NameTaken = "This name is already taken.";
            public const string WrongServiceType = "Wrong service type.";
            public const string WrongServiceDifficulty = "Wrong service difficulty.";
            public const string ServiceMissing = "There is no such service.";
        }

        public class Exercise
        {
            public const string NameTaken = "This name is already taken.";
            public const string WrongExerciseType = "Wrong exercise type.";
            public const string ExerciseMissing = "There is no such exercise.";
        }

        public class StatusCodes
        {
            public const HttpStatusCode Ok = HttpStatusCode.OK;
            public const HttpStatusCode Created = HttpStatusCode.Created;
            public const HttpStatusCode NoContent = HttpStatusCode.NoContent;

            public const HttpStatusCode BadRequest = HttpStatusCode.BadRequest;
            public const HttpStatusCode Forbidden = HttpStatusCode.Forbidden;
            public const HttpStatusCode NotFound = HttpStatusCode.NotFound;
            public const HttpStatusCode Conflict = HttpStatusCode.Conflict;

            public const HttpStatusCode InternalServerError = HttpStatusCode.InternalServerError;
        }
    }
}
