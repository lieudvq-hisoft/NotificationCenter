using System;
namespace Data.Models
{
    public class PickingRequestModel
    {
        public Guid Id { get; set; }
        public ProductModel Product { get; set; }
        public string? Note { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}

