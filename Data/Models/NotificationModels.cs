using System;
namespace Data.Models
{
	public class NotificationModel
	{
        public Guid Id { get; set; }
        public bool? Seen { get; set; }
        public string? Action { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public object? Data { get; set; }
        public string? TypeModel { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}

