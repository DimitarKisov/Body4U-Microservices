namespace Body4U.Common.Constants
{
    public class MessageConstants
    {
        public class Common
        {
            public const string EmailConfirmHtmlContent = "<p>To confirm your email, please click <a href=\"{0}\">HERE</a></p>";
            public const string EmailConfirmSubject = "Email Confirmation";
            public const string EmailProblem = "Email was not sent successfuly.";
            public const string Wrong = "Something went wrong in {0}.";
            public const string WrongImageFormat = "Please choose a picture with format .jpg/.jpeg or .png";
        }

        public class ApplicationUserConstants
        {
            public const string WrongGender = "There is no such gender.";
        }
    }
}
