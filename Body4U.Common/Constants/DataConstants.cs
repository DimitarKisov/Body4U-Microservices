namespace Body4U.Common.Constants
{
    public class DataConstants
    {
        public class ApplicationUser
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
            public const int ProfilePictureInProfileWidth = 200;
            public const int ProfilePictureInProfileHeight = 200;
            public const int ProfilePictureThumbnailWidth = 100;
            public const int ProfilePictureThumbailHeight = 100;
        }

        public class Article
        {
            public const int MinTitleLength = 5;
            public const int MaxTitleLength = 100;
            public const int MinContentLength = 50;
            public const int MaxContentLength = 25000;
            public const int MinImageWidth = 950;
            public const int MinImageHeight = 550;
        }

        public class Trainer
        {
            public const int MinBioLength = 200;
            public const int MaxBioLength = 500;
            public const int MinShortBioLength = 30;
            public const int MaxShortBioLength = 200;
            public const string FacebookUrlRegex = @"(?:(?:http|https):\/\/)?(?:www.)?facebook.com\/(?:(?:\w)*#!\/)?(?:pages\/)?(?:[?\w\-]*\/)?(?:profile.php\?id=(?=\d.*))?([\w.\-]*)?";
            public const int ImagesCountLimit = 6;
            public const string InstragramUrlRegex = @"^(?:(?:http|https):\/\/)?(?:www\.)?(?:instagram\.com|instagr\.am)\/([A-Za-z0-9-_\.]+)\/$";
            public const string YoutubeChannelUrlRegex = @"((http|https):\/\/|)(www\.|)youtube\.com\/((channel|c)\/|user\/)[a-zA-Z0-9\-]{1,}";
            public const string YoutubeVideoUrlRegex = @"^(?:https?\:\/\/)?(?:www\.)?(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v\=))([\w-]{10,12})(?:$|\&|\?\#).*";
            public const int TrainerImageWidth = 500;
            public const int TrainerImageHeight = 500;
        }

        public class Common
        {
            public const string AdministratorRoleName = "Administrator";
            public const string TrainerRoleName = "Trainer";
            public const string Desc = "desc";
        }

        public class Comment
        {
            public const int MinContentLength = 1;
            public const int MaxContentLength = 4000;
        }

        public class Service
        {
            public const int MinNameLength = 4;
            public const int MaxNameLength = 80;
            public const int MinDescriptionLength = 10;
            public const int MaxDescriptionLength = 2000;
            public const string MinPrice = "0";
            public const string MaxPrice = "5000";
        }

        public class Exercise
        {
            public const int MinNameLength = 3;
            public const int MaxNameLength = 50;
            public const int MaxDescriptionLength = 500;
        }

        public class Food
        {
            public const int MinNameLength = 3;
            public const int MaxNameLength = 100;
            public const string NameRegularExpression = "^([a-z ,A-Z]+|[а-я ,А-Я]+)$";
        }
    }
}
