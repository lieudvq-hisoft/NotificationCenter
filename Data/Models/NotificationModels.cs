using System;
namespace Data.Models
{
	public class NotificationModel
	{
	}
    public class NotificationCreateModel
    {
        public bool? Seen { get; set; }
        public string? Action { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public object? Data { get; set; }
    }
}

