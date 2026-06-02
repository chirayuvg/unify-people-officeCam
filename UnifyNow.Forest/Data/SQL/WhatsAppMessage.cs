using System.ComponentModel.DataAnnotations;
using static UnifyNow.Forest.Controllers.APIController.WhatsAppMediaController;

namespace UnifyNow.Forest.Data.SQL
{

    public class WhatsAppMessage  
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string? Name { get; set; }

        public string RecipientNumber { get; set; } // Receiver's WhatsApp number

      //  public string? VisitorNumber { get; set; } // Receiver's WhatsApp number


        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double? Longitude { get; set; }

        [MaxLength(250)]
        [DataType(DataType.Upload)]
        public string? file { get; set; }
        public string MessageText { get; set; } // Message content
        public string? Address { get; set; }
       // public List<Contact>? Contacts { get; set; }


    }
}
