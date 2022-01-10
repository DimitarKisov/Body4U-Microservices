namespace Body4U.Common.Messages
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Message
    {
        public Message()
        {
        }

        public Message(object data)
        {
            this.SerializeData(data);
        }

        public int Id { get; private set; }

        [Required]
        public string Data { get; private set; }

        [Required]
        public Type Type { get; private set; }

        public bool Published { get; private set; }

        public void MarkAsPublished() => this.Published = true;

        private void SerializeData(object data)
        {
            this.Type = data.GetType();
            this.Data = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public object DeserializeData()
        {
            return JsonConvert.DeserializeObject(this.Data, this.Type, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}
