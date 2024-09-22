namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{

    public class BookingVM
    {
        public string BookingNumber { get; set; }
        public int TableNumber { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
        public string Email { get; set; }
        public int AmountOfGuests { get; set; }
    }
}
