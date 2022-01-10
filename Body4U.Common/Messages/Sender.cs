namespace Body4U.Common.Messages
{
    using System;

    public class Sender
    {
        public Sender()
        {
            this.Identifier = Guid.NewGuid().ToString();
        }

        public string Identifier { get; private set; }
    }
}
