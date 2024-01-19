using System;
namespace Data.Models
{
    public class OrderModel
    {
        public Guid Id { get; set; }
        public Guid SentBy { get; set; }
        public UserModel? SentByUser { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public List<string>? Files { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}

