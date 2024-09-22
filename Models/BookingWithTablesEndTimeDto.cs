namespace Lab2_MVC_Resto_Frontend.Models
{
    public class BookingWithTablesEndTimeDto
    {
        public string BookingNumber { get; set; }
        public string Email { get; set; }
        public DateTime TimeStamp { get; set; }
        public int AmountOfGuests { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }

        public ICollection<TableDto>? Tables { get; set; }
    }
}

