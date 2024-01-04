using System;
namespace Data.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? SalePrice { get; set; }
        public string? SerialNumber { get; set; }
        public string? InternalCode { get; set; }
        public List<string>? Images { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}

