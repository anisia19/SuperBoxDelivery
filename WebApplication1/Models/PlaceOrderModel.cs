using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class PlaceOrderModel
    {
        [Required]
        public string SuperBoxId { get; set; }

        [Required]
        public int? ReceiverUserId { get; set; }

        [Required]
        public string ReceiverSuperBoxId { get; set; }

        [Required]
        public bool IsUrgent { get; set; }

        [Required]
        public string RelevantInfo { get; set; }
        public int UserId { get; set; }
        
    }
}