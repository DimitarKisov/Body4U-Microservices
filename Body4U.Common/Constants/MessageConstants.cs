namespace Body4U.Common.Constants
{
    public class MessageConstants
    {
        public class Common
        {
            public const string EmailConfirmHtmlContent = "<p>To confirm your email, please click <a href=\"{0}\">HERE</a></p>";
            public const string EmailConfirmSubject = "Email Confirmation";
            public const string EmailProblem = "Email was not sent successfuly.";
            public const string ForgotPasswordHtmlContent = "<p>To reset your password, please click <a href=\"{0}\">HERE</a></p>";
            public const string ForgotPasswordSubject = "Password Reset";
            public const string InternalServerError = "Internal server error.";
            public const string Wrong = "Something went wrong in {0}.";
            public const string WrongImageFormat = "Please choose a picture with format .jpg/.jpeg or .png";
            public const string WrongWrights = "You don't have the rights for this action!";
        }

        public class ApplicationUserConstants
        {
            public const string EmailNotConfirmed = "Email is not confirmed.";
            public const string Locked = "Account is locked.";
            public const string LoginFailed = "Login failed.";
            public const string UserNotFound = "There is not such user with Id '{0}'";
            public const string WrongGender = "There is no such gender.";
            public const string WrongUsernameOrPassword = "Wrong email or password.";
        }

        public class TrainerConstants
        {
            public const string TrainerNotFound = "User with ID '{0}' is not a trainer.";
        }
    }
}
