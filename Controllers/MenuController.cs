using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Lab2_MVC_Resto_Frontend.Controllers
{
    public class MenuController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://localhost:7212/api/";

        public MenuController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
        }
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var menu = JsonSerializer.Deserialize<IEnumerable<MealCategoryWithMealsVM>>(response,options);
            return View(menu);
        }
    }
}
