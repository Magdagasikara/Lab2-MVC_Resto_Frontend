using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Lab2_MVC_Resto_Frontend.Controllers
{
    public class MenuController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://localhost:7212/api/";
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public MenuController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
        }
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu");
            //var options = new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //};

            var menu = JsonSerializer.Deserialize<IEnumerable<MealCategoryWithMealsVM>>(response, _options);
            return View(menu);
        }
        public async Task<IActionResult> DealOfTheDay()
        {
            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu");
            //var options = new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //};

            var menu = JsonSerializer.Deserialize<IEnumerable<MealCategoryWithMealsVM>>(response, _options);
            // Skicka med modellen till partial view - var det okej?
            // välj slumpmässigt en rätt från första tre kategorier
            var meals = menu.Select(m => m.Meals).ToList();
            return PartialView("_DealOfTheDay", meals);
        }
        // admin
        public ActionResult AddMealCategory()
        {
            // skicka titel på sidan?
            // skapa formulär
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddMealCategory(MealCategoryVM category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            // funkar det med PostAsJsonAsync?? ingen encoding här.
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}menu/categories/category/add", category);
            // kanske hämta status code?
            // och 200-serie så till "you added XXX"?
            return RedirectToAction("Index");
        }
        // admin
        public IActionResult AddMeal()
        {
            // sidtitel
            // formulär
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddMeal(MealVM meal)
        {

            if (!ModelState.IsValid)
            {
                return View(meal);
            }
            var json = JsonSerializer.Serialize(meal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUri}menu/meals/meal/add", content);

            // kanske kolla status code, om 2xx då "success! wow!" etc
            return RedirectToAction("Index");
        }
        // admin
        public async Task<IActionResult> MealCategories()
        {
            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu/categories");
            var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(response, _options);

            return View(categories);
        }
        // admin
        public async Task<IActionResult> Meals()
        {
            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu/meals");
            var meals = JsonSerializer.Deserialize<IEnumerable<MealDetailVM>>(response, _options);
            return View(meals);
        }
        // admin
        public async Task<IActionResult> EditCategory()
        {
            return View();
        }
        // admin
        public async Task<IActionResult> EditMeal()
        {
            return View();
        }
        // admin
        public async Task<IActionResult> RemoveCategory()
        {
            return View();
        }
        // admin
        public async Task<IActionResult> RemoveMeal()
        {
            return View();
        }

    }

}
