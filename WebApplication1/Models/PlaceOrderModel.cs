using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class PlaceOrderModel
    {
        [Required]
        public string SuperBoxId { get; set; }

        [Required]
        public string ReceiverSuperBoxId { get; set; }

        [Required]
        public bool IsUrgent { get; set; }
        [MaxLength(100, ErrorMessage = "The field cannot exceed 100 characters.")]
        public string? RelevantInfo { get; set; }


        // This will hold the username input to find the receiver's ID
        [Required]
        public string ReceiverUserName { get; set; }

        // This field will be set after finding the user in the controller, not from the form
        public int? ReceiverUserId { get; set; }

        // You can keep or remove this based on whether you want to track the current user's ID
        public int? UserId { get; set; }
    }
}