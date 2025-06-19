using System.ComponentModel.DataAnnotations;

namespace MenuShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Stock { get; set; } = 0;
        
        // Giảm giá
        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0 đến 100")]
        public decimal DiscountPercent { get; set; } = 0;
        
        public DateTime? DiscountStartDate { get; set; }
        
        public DateTime? DiscountEndDate { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? VideoUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Foreign key
        public int CategoryId { get; set; }
        
        // Navigation property
        public virtual Category? Category { get; set; }
        
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        
        // Tính toán giá sau giảm
        public decimal FinalPrice
        {
            get
            {
                if (!IsDiscountActive)
                    return Price;
                
                var discountAmount = Price * (DiscountPercent / 100);
                return Price - discountAmount;
            }
        }
        
        // Kiểm tra xem giảm giá có đang hoạt động không
        public bool IsDiscountActive
        {
            get
            {
                if (DiscountPercent <= 0)
                    return false;
                
                var now = DateTime.Now;
                var startDate = DiscountStartDate ?? DateTime.MinValue;
                var endDate = DiscountEndDate ?? DateTime.MaxValue;
                
                return now >= startDate && now <= endDate;
            }
        }
        
        // Số tiền được giảm
        public decimal DiscountAmount
        {
            get
            {
                if (!IsDiscountActive)
                    return 0;
                
                return Price * (DiscountPercent / 100);
            }
        }
    }
} 