using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1.Models
{
    public enum OrderStatus
    {
        InLocker,
        Delivered,
        Canceled
    }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [ForeignKey("User")]
        public int? UserId { get; set; }
        [BindNever]
        public User? User { get; set; }
        [Required(ErrorMessage = "Please select a SuperBox.")]
        public string? SuperBoxId { get; set; }
        [BindNever]
        public SuperBox? SuperBox { get; set; }
        [ForeignKey("ReceiverSuperBox")]
        public string? ReceiverSuperBoxId { get; set; }
        [BindNever]
        public SuperBox? ReceiverSuperBox { get; set; }
        [ForeignKey("ReceiverUser")]
        public int? ReceiverUserId { get; set; }
        [BindNever]
        public User? ReceiverUser { get; set; }
        public bool IsUrgent { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.InLocker;
        [MaxLength(100, ErrorMessage = "The field cannot exceed 100 characters.")]
        public string? RelevantInfo { get; set; }

        public Order() { }
        public Order(User user, bool isUrgent, OrderStatus status, string relevantInfo, string superBoxId, string receiverSuperBoxId, int? receiverUserId)
        {
            User = user;
            IsUrgent = isUrgent;
            Status = status;
            RelevantInfo = relevantInfo;
            SuperBoxId = superBoxId;
            ReceiverSuperBoxId = receiverSuperBoxId;
            ReceiverUserId = receiverUserId;
        }
    }
}
