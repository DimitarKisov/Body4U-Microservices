namespace Body4U.Common.Constants
{
    public class DataConstants
    {
        public class Common
        {
            public const string AdministratorRoleName = "Administrator";
        }

        public class ApplicationUserConstants
        {
            public const int FirstNameMinLenght = 3;
            public const int FirstNameMaxLength = 50;
            public const int LastNameMinLength = 3;
            public const int LastNameMaxLength = 50;
            public const string PhoneNumberRegex = @"^(\+)?(359|0)8[789]\d{1}(|-| )\d{3}(|-| )\d{3}$";
            public const int MinAge = 5;
            public const int MaxAge = 80;
            public const int MinPasswordLength = 6;
            public const int MaxPasswordLength = 20;
        }
    }
}
