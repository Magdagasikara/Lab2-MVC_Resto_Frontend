using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu");
            var menu = JsonSerializer.Deserialize<IEnumerable<MealCategoryWithMealsVM>>(response, _options);
            return View(menu);
        }
        public async Task<IActionResult> DealOfTheDay()
        {

            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu");
            var menu = JsonSerializer.Deserialize<IEnumerable<MealCategoryWithMealsVM>>(response, _options);
            // try - catch och skriv ut i konsolen genom ViewData?
            return PartialView("_DealOfTheDay", menu);
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
        public async Task<IActionResult> AddMeal()
        {
            // get categories to choose among
            var response = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu/categories");
            var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(response, _options);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddMeal(MealAddVM meal)
        {

            if (!ModelState.IsValid)
            {
                return View(meal);
            }
            var json = JsonSerializer.Serialize(meal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUri}menu/meals/meal/add", content);
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
        public async Task<IActionResult> EditMeal(int mealId)
        {
            // get info on the chose meal
            // as long as I dont transfer the meal model from Meal List to Edit I make a GET request
            var responseMeal = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu/meals/meal/{mealId}");
            var meal = JsonSerializer.Deserialize<MealDetailVM>(responseMeal, _options);

            // get categories to choose among
            var responseCategories = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu/categories");
            var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(meal);
        }
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
            var response = await _httpClient.PatchAsync($"{_httpClient.BaseAddress}menu/meals/meal/update", content);
            //var response = await _httpClient.PatchAsJsonAsync($"{_httpClient.BaseAddress}menu/meals/meal/update", meal);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"{meal.Name} successfully updated!";
                return RedirectToAction("Meals");
            }
            else
            {
                // Apparently ViewBag needs to be refilled in case of error
                var responseCategories = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}menu/categories");
                var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                ViewData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                return View(meal);
            }
        }
        // admin
        public async Task<IActionResult> RemoveCategory()
        {
            return View();
        }
        // admin
        public IActionResult RemoveMeal(int mealId)
        {
            return View(mealId);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveMealConfirm(int mealId)
        {
            var response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}menu/meals/meal/{mealId}/delete");
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
