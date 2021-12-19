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
            public const string InternalServerError = "Internal server error.";
            public const string MultipleNoImagesChosen = "There isn't any images chosen.";
            public const string MultipleWrongImageFormats = "There is a unsupported format in your images.";
            public const string MultipleWrongWidthOrHeight = "There are images with width under {0} or height under {1} pixels.";
            public const string NoImage = "There is no image chosen.";
            public const string Wrong = "Something went wrong in {0}.";
            public const string WrongImageFormat = "Please choose a picture with format .jpg/.jpeg or .png";
            public const string WrongWidthOrHeight = "Please upload a image with min width: {0} and min height: {1}.";
            public const string WrongWrights = "You don't have the rights for this action!";
        }

        public class Article
        {
            public const string TitleTaken = "This title is already taken.";
            public const string WrongArticleType = "Wrong article type.";
        }

        public class ApplicationUser
        {
            public const string EmailNotConfirmed = "Email is not confirmed.";
            public const string Locked = "Account is locked.";
            public const string LoginFailed = "Login failed.";
            public const string ProfilePictureNotFound = "No profile picture found to change.";
            public const string TrainerNotFound = "User with ID '{0}' is not a trainer.";
            public const string UserNotFound = "There is not such user with Id '{0}'";
            public const string WrongGender = "There is no such gender.";
            public const string WrongUsernameOrPassword = "Wrong email or password.";
        }

        public class Trainer
        {
            public const string MaxAllowedImages = "The max allowed images you can upload is {0}";
            public const string TooManyImages = "The max allowed images is {0}. You can upload {1} more images.";
            public const string TrainerNotFound = "User is not a trainer.";
        }
    }
}
