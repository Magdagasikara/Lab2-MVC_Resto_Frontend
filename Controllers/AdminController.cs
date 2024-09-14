using Microsoft.AspNetCore.Mvc;

namespace Lab2_MVC_Resto_Frontend.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
    }
}
