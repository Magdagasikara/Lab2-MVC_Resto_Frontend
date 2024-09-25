namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{
    public class TableVM
    {
        public int TableNumber { get; set; }
        public int AmountOfPlaces { get; set; }
        public List<BookingVM> Bookings { get; set; }
    }
}
