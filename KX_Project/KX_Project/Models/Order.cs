using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KX_Project.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; } = null!;

        // Trạng thái đơn hàng: "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
        public string Status { get; set; } = "Pending";

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
