using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
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
            var menu = new List<MealCategoryWithMealsVM>();
            try
            {
                var response = await _httpClient.GetStringAsync("menu");
                menu = JsonSerializer.Deserialize<List<MealCategoryWithMealsVM>>(response, _options);
            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Just come by and try instead of checking things online!";
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Just come by and try instead of checking things online!";
            }
            return View(menu);
        }
        public async Task<IActionResult> DealOfTheDay()
        {
            var menu = new List<MealCategoryWithMealsVM>();
            try
            {
                var response = await _httpClient.GetStringAsync("menu");
                menu = JsonSerializer.Deserialize<List<MealCategoryWithMealsVM>>(response, _options);
            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Just come by and try instead of checking things online!";
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Just come by and try instead of checking things online!";
            }
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
            // no view for this one, possible improvement!
            // add try-catch then

            if (!ModelState.IsValid)
            {
                return View(category);
            }
            var token = HttpContext.Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsJsonAsync("menu/categories/category/add", category);
            return RedirectToAction("Index");
        }
        [Authorize]
        // admin
        public async Task<IActionResult> AddMeal()
        {
            try
            {
                // get categories to choose among
                var token = HttpContext.Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetStringAsync("menu/categories");
                var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(response, _options);
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
                return View();
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return View();
            }
            return View();
        }
        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> AddMeal(MealAddVM meal)
        {
            try
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
                }
            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }

            return View(meal);
        }
        [Authorize]
        // admin
        public async Task<IActionResult> MealCategories()
        {

            // no view for this one, possible improvement!
            // add try-catch then

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
            var meals = new List<MealDetailVM>();
            try
            {
                var token = HttpContext.Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetStringAsync("menu/meals");
                meals = JsonSerializer.Deserialize<List<MealDetailVM>>(response, _options);
            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }
            return View(meals);
        }

        [Authorize]
        // admin
        public async Task<IActionResult> EditCategory()
        {
            // not implemented, possible improvement!
            // add try-catch then
            return View();
        }

        [Authorize]
        // admin
        public async Task<IActionResult> EditMeal(int mealId)
        {
            var meal = new MealDetailVM();
            ViewBag.Categories = new List<MealCategoryVM>();
            try
            {
                // First fetch info on the chosen meal (I could transfer the meal model here but now I make a GET request with id instead)
                var token = HttpContext.Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var responseMeal = await _httpClient.GetStringAsync($"menu/meals/meal/{mealId}");
                meal = JsonSerializer.Deserialize<MealDetailVM>(responseMeal, _options);

                // get categories to choose among
                var responseCategories = await _httpClient.GetStringAsync("menu/categories");
                var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }
            return View(meal);
        }

        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> EditMeal(MealDetailVM meal)
        {
            try
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
                }
            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }
            return View(meal);
        }

        [Authorize]
        // admin
        public async Task<IActionResult> RemoveCategory()
        {
            // not implemented, possible improvement!
            // add try-catch then
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
            try { 
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
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
                return RedirectToAction("Meals");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Meals");
            }

        }
    }

}
