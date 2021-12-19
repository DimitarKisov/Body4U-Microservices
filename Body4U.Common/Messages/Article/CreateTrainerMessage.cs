namespace Body4U.Common.Messages.Article
{
    using System;

    public class CreateTrainerMessage
    {
        public string ApplicationUserId { get; set; }
        
        public DateTime CreatedOn { get; set; }
    }
}
