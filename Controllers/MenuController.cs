using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Reflection;
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
            var response = await _httpClient.GetStringAsync("menu");
            var menu = JsonSerializer.Deserialize<IEnumerable<MealCategoryWithMealsVM>>(response, _options);
            return View(menu);
        }
        public async Task<IActionResult> DealOfTheDay()
        {

            var response = await _httpClient.GetStringAsync("menu");
            var menu = JsonSerializer.Deserialize<IEnumerable<MealCategoryWithMealsVM>>(response, _options);
            // try - catch och skriv ut i konsolen genom ViewData?
            return PartialView("_DealOfTheDay", menu);
        }
        [Authorize]
        // admin
        public ActionResult AddMealCategory()
        {
            return View();
        }
        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> AddMealCategory(MealCategoryVM category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // funkar det med PostAsJsonAsync?? ingen encoding här.
            var response = await _httpClient.PostAsJsonAsync("menu/categories/category/add", category);
            // kanske hämta status code?
            // och 200-serie så till "you added XXX"?
            return RedirectToAction("Index");
        }
        [Authorize]
        // admin
        public async Task<IActionResult> AddMeal()
        {
            // get categories to choose among
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetStringAsync("menu/categories");
            var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(response, _options);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }
        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> AddMeal(MealAddVM meal)
        {

            if (!ModelState.IsValid)
            {
                return View(meal);
            }
            var json = JsonSerializer.Serialize(meal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsync("menu/meals/meal/add", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"{meal.Name} successfully added!";
                return RedirectToAction("Meals");
            }
            else
            {
                ViewData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                return View(meal);
            }
        }
        [Authorize]
        // admin
        public async Task<IActionResult> MealCategories()
        {
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetStringAsync("menu/categories");
            var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(response, _options);

            return View(categories);
        }
        [Authorize]
        // admin
        public async Task<IActionResult> Meals()
        {
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetStringAsync("menu/meals");
            var meals = JsonSerializer.Deserialize<IEnumerable<MealDetailVM>>(response, _options);
            return View(meals);
        }
        [Authorize]
        // admin
        public async Task<IActionResult> EditCategory()
        {
            return View();
        }
        [Authorize]
        // admin
        public async Task<IActionResult> EditMeal(int mealId)
        {
            // get info on the chose meal
            // as long as I dont transfer the meal model from Meal List to Edit I make a GET request
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMeal = await _httpClient.GetStringAsync($"menu/meals/meal/{mealId}");
            var meal = JsonSerializer.Deserialize<MealDetailVM>(responseMeal, _options);

            // get categories to choose among
            var responseCategories = await _httpClient.GetStringAsync("menu/categories");
            var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(meal);
        }
        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> EditMeal(MealDetailVM meal)
        {
            if (!ModelState.IsValid)
            {
                return View(meal);
            }

            var json = JsonSerializer.Serialize(meal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PatchAsync("menu/meals/meal/update", content);
            //var response = await _httpClient.PatchAsJsonAsync("menu/meals/meal/update", meal);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"{meal.Name} successfully updated!";
                return RedirectToAction("Meals");
            }
            else
            {
                // Apparently ViewBag needs to be refilled in case of error
                var responseCategories = await _httpClient.GetStringAsync("menu/categories");
                var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                ViewData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                return View(meal);
            }
        }
        [Authorize]
        // admin
        public async Task<IActionResult> RemoveCategory()
        {
            return View();
        }
        [Authorize]
        // admin
        public IActionResult RemoveMeal(int mealId)
        {
            return View(mealId);
        }
        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> RemoveMealConfirm(int mealId)
        {
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"menu/meals/meal/{mealId}/delete");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Meal successfully deleted!";
                return RedirectToAction("Meals");
            }
            else
            {
                TempData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                return RedirectToAction("Meals");
            }
        }
    }

}
