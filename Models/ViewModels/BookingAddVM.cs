using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{

    public class BookingAddVM
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please fill in correct Email-address")]
        [StringLength(100, MinimumLength = 6)]
        public string Email { get; set; }
        [Required]
        [DisplayName("Number of guests")]
        [Range(1, 1000, ErrorMessage = "Number of guests must be minimum 1")] 
        public int AmountOfGuests { get; set; }
        [Required]
        [DisplayName("Reservation start")]
        public DateTime ReservationStart { get; set; }
        [Required]
        [DisplayName("Reservation time in hours")]
        [Range(1, 10, ErrorMessage = "Reservation time must be minimum 1 hour")]
        public double ReservationDurationInHours { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }

    }
}
