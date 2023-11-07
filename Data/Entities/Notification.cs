using System;
namespace Data.Entities
{
    public class Notification : BaseEntity
    {
        public bool Seen { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public object Data { get; set; }
    }
}