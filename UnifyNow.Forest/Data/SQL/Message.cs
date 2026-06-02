using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class Message
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string Sender { get; set; }

        public string Receiver { get; set; }

        public string MessageText { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        public Guid? TicketId { get; set; }
        public Ticket? Ticket { get; set; }

    }
}
