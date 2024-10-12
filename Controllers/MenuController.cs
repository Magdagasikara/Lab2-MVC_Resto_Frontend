using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MenuController> _logger;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public MenuController(HttpClient httpClient, ILogger<MenuController> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var menu = new List<MealCategoryWithMealsVM>();
            try
            {
                _logger.LogInformation($"Fetching menu at time {DateTime.Now}.");
                var response = await _httpClient.GetStringAsync("menu");
                menu = JsonSerializer.Deserialize<List<MealCategoryWithMealsVM>>(response, _options);
                _logger.LogInformation($"Menu fetched with {menu.Count()} categories at time {DateTime.Now}.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when fetching menu at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Just come by and try instead of checking things online!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when fetching menu at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "An unexpected error occurred. Just come by and try instead of checking things online!";
            }
            return View(menu);
        }
        public async Task<IActionResult> DealOfTheDay()
        {
            var menu = new List<MealCategoryWithMealsVM>();
            try
            {
                _logger.LogInformation($"Fetching menu for deal of the day at time {DateTime.Now}.");
                var response = await _httpClient.GetStringAsync("menu");
                menu = JsonSerializer.Deserialize<List<MealCategoryWithMealsVM>>(response, _options);
                _logger.LogInformation($"Menu fetched with {menu.Count()} categories at time {DateTime.Now}.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when fetching menu at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Just come by and try instead of checking things online!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when fetching menu at time {DateTime.Now}: {ex.Message}.");
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
                _logger.LogWarning($"Invalid MealCategory model received: {ModelState.ErrorCount}", ModelState.Values.SelectMany(v => v.Errors));
                return View(category);
            }
            
            var token = HttpContext.Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation($"Posting new category at time {DateTime.Now}.");
            var response = await _httpClient.PostAsJsonAsync("menu/categories/category/add", category);
            _logger.LogInformation($"New category posted at time {DateTime.Now}.");

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
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"Fetching categories to display when adding a meal at time {DateTime.Now}.");
                var response = await _httpClient.GetStringAsync("menu/categories");
                var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(response, _options);
                _logger.LogInformation($"Categories (amount = {categories.Count()}) fetched at time {DateTime.Now}.");

                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when fetching categories at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when fetching categories at time {DateTime.Now}: {ex.Message}.");
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
                    _logger.LogWarning($"Invalid MealAddVM model received: {ModelState.ErrorCount}", ModelState.Values.SelectMany(v => v.Errors));
                    
                    // Apparently ViewBag needs to be refilled in case of error
                    _logger.LogInformation($"Fetching categories to choose among in meal edit at time {DateTime.Now}.");
                    var responseCategories = await _httpClient.GetStringAsync("menu/categories");
                    var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
                    _logger.LogInformation($"Categories (amount = {categories.Count()}) fetched at time {DateTime.Now}.");

                    ViewBag.Categories = new SelectList(categories, "Id", "Name");

                    return View(meal);
                }
                var json = JsonSerializer.Serialize(meal);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var token = HttpContext.Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"Adding a meal at time {DateTime.Now}.");
                var response = await _httpClient.PostAsync("menu/meals/meal/add", content);
                _logger.LogInformation($"Meal added at time {DateTime.Now}.");

                //if (response.IsSuccessStatusCode)
                //{
                    TempData["SuccessMessage"] = $"{meal.Name} successfully added!";
                    return RedirectToAction("Meals");
                //}
                //else
                //{
                //    ViewData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                //}
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when fetching categories to meal edit or posting a new meal {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when posting a new meal at time {DateTime.Now}: {ex.Message}.");
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
            if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetStringAsync("menu/categories");
            var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(response, _options);
            _logger.LogInformation($"Categories (amount = {categories.Count()}) fetched at time {DateTime.Now}.");

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
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetStringAsync("menu/meals");
                meals = JsonSerializer.Deserialize<List<MealDetailVM>>(response, _options);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when fetching all meals {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when fetching all meals at time {DateTime.Now}: {ex.Message}.");
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
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                _logger.LogInformation($"Fetching meal at time {DateTime.Now}.");
                var responseMeal = await _httpClient.GetStringAsync($"menu/meals/meal/{mealId}");
                meal = JsonSerializer.Deserialize<MealDetailVM>(responseMeal, _options);

                // get categories to choose among
                _logger.LogInformation($"Fetching categories to choose among in meal edit at time {DateTime.Now}."); 
                var responseCategories = await _httpClient.GetStringAsync("menu/categories");
                var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
                _logger.LogInformation($"Categories (amount = {categories.Count()}) fetched at time {DateTime.Now}.");
            
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when fetching info about the meal to edit or when fetching the category list, at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when fetching info about the meal to edit or when fetching the category list, at time {DateTime.Now}: {ex.Message}.");
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
                    _logger.LogWarning($"Invalid MealDetailVM model received: {ModelState.ErrorCount}", ModelState.Values.SelectMany(v => v.Errors));
                    return View(meal);
                }

                var json = JsonSerializer.Serialize(meal);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var token = HttpContext.Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"Updating meal at time {DateTime.Now}.");
                var response = await _httpClient.PatchAsync("menu/meals/meal/update", content);
                //var response = await _httpClient.PatchAsJsonAsync("menu/meals/meal/update", meal);
                _logger.LogInformation($"Meal updated at time {DateTime.Now}.");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"{meal.Name} successfully updated!";
                    return RedirectToAction("Meals");
                }
                else
                {
                    // Apparently ViewBag needs to be refilled in case of error
                    _logger.LogInformation($"Fetching categories to choose among in meal edit at time {DateTime.Now}."); 
                    var responseCategories = await _httpClient.GetStringAsync("menu/categories");
                    var categories = JsonSerializer.Deserialize<IEnumerable<MealCategoryVM>>(responseCategories, _options);
                    _logger.LogInformation($"Categories (amount = {categories.Count()}) fetched at time {DateTime.Now}.");
            
                    ViewBag.Categories = new SelectList(categories, "Id", "Name");

                    ViewData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when updating a meal {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when updating a meal at time {DateTime.Now}: {ex.Message}.");
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
            try
            {
                var token = HttpContext.Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"Deleting the meal at time {DateTime.Now}.");
                var response = await _httpClient.DeleteAsync($"menu/meals/meal/{mealId}/delete");
                _logger.LogInformation($"Meal deleted at time {DateTime.Now}.");


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
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when deleting the meal at time {DateTime.Now}: {ex.Message}.");
                TempData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
                return RedirectToAction("Meals");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when deleting the meal at time {DateTime.Now}: {ex.Message}.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Meals");
            }

        }
    }

}
