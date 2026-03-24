using System.ComponentModel.DataAnnotations;

namespace DoanCStar.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } 
        [Required]
        public string Name { get; set; }
            
        public string Description { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } 

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Active { get; set; }
    }
}