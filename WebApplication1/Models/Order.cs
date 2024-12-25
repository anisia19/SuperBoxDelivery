using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public enum OrderStatus
    {
        InLocker,
        Delivered
    }

    public class Order
    {
        [Key]
        public int IdOrder { get; set; }
        public User User { get; set; }
        public string SuperBoxId { get; set; }

        public List<SuperBox> SuperBoxOptions{ get; set; }
        public bool IsUrgent { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.InLocker;
        public string RelevantInfo { get; set; }
        public Order() { }
        public Order(User user,bool isUrgent, OrderStatus status, string relevantInfo, string superBoxId)
        {
            User = user;
            IsUrgent = isUrgent;
            Status = status;
            RelevantInfo = relevantInfo;
            SuperBoxId = superBoxId;
        }
    }
}
