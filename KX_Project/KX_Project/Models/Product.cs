using System.ComponentModel.DataAnnotations;

namespace KX_Project.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        
        // Thông số kỹ thuật
        public string? Processor { get; set; }
        public string? Screen { get; set; }
        public string? Camera { get; set; }
        public string? Battery { get; set; }
        public string? Storage { get; set; }
        public string? Os { get; set; }
        
        // Laptop & Desktop specs
        public string? Ram { get; set; }
        public string? GraphicsCard { get; set; }
        public string? Weight { get; set; }

        // Audio specs (Tai nghe, DAC)
        public string? Connectivity { get; set; }
        public string? FrequencyResponse { get; set; }
        public string? Impedance { get; set; }
        public string? Sensitivity { get; set; }
        public string? AudioFormat { get; set; }

        public List<ProductImage>? Images { get; set; }
        public Category? Category { get; set; }
    }
}
