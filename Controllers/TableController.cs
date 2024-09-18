using Microsoft.AspNetCore.Mvc;

namespace Lab2_MVC_Resto_Frontend.Controllers
{
    public class TableController : Controller
    {

        // hela controller baraför admin
        public IActionResult Index()
        {
            return View();
        }

        // admin
        public async Task<IActionResult> AddBooking()
        {
            return View();
        }
        // admin
        public async Task<IActionResult> GetBookings()
        {
            return View();
        }
        // admin
        public async Task<IActionResult> UpdateBooking()
        {
            return View();
        }
        // admin
        public async Task<IActionResult> DeleteBooking()
        {
            return View();
        }
    }
}
