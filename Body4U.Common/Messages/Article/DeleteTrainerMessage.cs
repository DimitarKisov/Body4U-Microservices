namespace Body4U.Common.Messages.Article
{
    using System;

    public class DeleteTrainerMessage : Sender
    {
        public string ApplicationUserId { get; set; }
    }
}
