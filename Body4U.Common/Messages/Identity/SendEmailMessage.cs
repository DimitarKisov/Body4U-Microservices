namespace Body4U.Common.Messages.Identity
{
    public class SendEmailMessage : Sender
    {
        public string To { get; set; }

        public string Subject { get; set; }

        public string HtmlContent { get; set; }
    }
}
