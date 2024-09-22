namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{
    public class ScheduleVM
    {
        public List<TableVM> Tables { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
