namespace Body4U.Common.Messages.Identity
{
    public class EditUserNamesMessage
    {
        public string ApplicationUserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ModifiedBy { get; set; }
    }
}
