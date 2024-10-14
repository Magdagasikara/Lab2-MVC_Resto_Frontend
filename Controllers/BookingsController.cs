using Lab2_MVC_Resto_Frontend.Models;
using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Lab2_MVC_Resto_Frontend.Controllers
{
    public class BookingsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://localhost:7212/api/";
        private readonly ILogger<BookingsController> _logger;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public BookingsController(HttpClient httpClient, ILogger<BookingsController> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
            _logger = logger;
        }

        //[Authorize(Roles = "Admin")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var schedule = new ScheduleVM();

            try
            {
                var token = HttpContext.Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Get bookings from API
                var responseBookings = await _httpClient.GetStringAsync("bookings");
                var bookings = JsonSerializer.Deserialize<IEnumerable<BookingWithTablesEndTimeDto>>(responseBookings, _options);

                // Show bookings from now and one week ahead
                var startDate = DateTime.Today.AddMinutes(Math.Ceiling((DateTime.Now - DateTime.Today).TotalMinutes / 30) * 30 - 60);
                var endDate = DateTime.Today.AddDays(7);

                // Get restaurant's tables from API
                var responseTables = await _httpClient.GetStringAsync("tables");
                var tables = JsonSerializer.Deserialize<List<TableVM>>(responseTables, _options);

                foreach (var table in tables)
                {
                    table.Bookings = bookings
                            .Where(b =>
                                b.Tables != null
                                && b.Tables.Any(t => t.TableNumber == table.TableNumber)
                                && b.ReservationStart >= DateTime.Today
                                )
                            .Select(b => new BookingVM
                            {
                                BookingNumber = b.BookingNumber,
                                Email = b.Email,
                                AmountOfGuests = b.AmountOfGuests,
                                ReservationStart = b.ReservationStart,
                                ReservationEnd = b.ReservationEnd,
                                TableNumber = table.TableNumber
                            }).ToList();
                };

                // Fill in the schedule
                schedule = new ScheduleVM
                {
                    Tables = tables,
                    StartDate = startDate,
                    EndDate = endDate
                };

            }
            catch (HttpRequestException)
            {
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }
            return View(schedule);
        }

        public async Task<IActionResult> BookATable()
        {
            return View();
        }
        [Authorize]
        // admin
        public async Task<IActionResult> AddBooking()
        {
            return View();
        }

        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> AddBooking(BookingAddVM booking)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(booking);
                }
                booking.TimeStamp = DateTime.Now;

                var json = JsonSerializer.Serialize(booking);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var token = HttpContext.Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsync("bookings/booking/add", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Reservation for {booking.AmountOfGuests} on {booking.ReservationStart} successfully created. We did not send a confirmation mail to {booking.Email}.";
                    return RedirectToAction("Index");
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

            return View(booking);
        }

        [Authorize]
        // admin
        public async Task<IActionResult> UpdateBooking(string bookingNumber)
        {
            var bookingToUpdate = new BookingUpdateVM();
            try
            {
                // First fetch info on the chosen meal (I could transfer the meal model here but now I make a GET request with id instead)
                var token = HttpContext.Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"Fetching booking at time {DateTime.Now}.");
                var response = await _httpClient.GetStringAsync($"bookings/booking/{bookingNumber}/simple");
                var booking = JsonSerializer.Deserialize<BookingVM>(response, _options);
                _logger.LogInformation($"Booking fetched at time {DateTime.Now}.");

                TimeSpan duration = booking.ReservationEnd - booking.ReservationStart;
                double reservationDurationInHours = duration.TotalHours;

                bookingToUpdate = new BookingUpdateVM()
                {
                    BookingNumber = booking.BookingNumber,
                    AmountOfGuests = booking.AmountOfGuests,
                    Email = booking.Email,
                    ReservationStart = booking.ReservationStart,
                    ReservationDurationInHours = reservationDurationInHours
                };

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when fetching booking at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when fetching booking at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }
            return View(bookingToUpdate);

        }
        [HttpPost]
        [Authorize]
        // admin
        public async Task<IActionResult> UpdateBooking(BookingUpdateVM booking)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid BookingUpdateVM model received: {ModelState.ErrorCount}", ModelState.Values.SelectMany(v => v.Errors));
                    return View(booking);
                }

                var json = JsonSerializer.Serialize(booking);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var token = HttpContext.Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token)) _logger.LogWarning("JWT token is missing in the request.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"Updating booking at time {DateTime.Now}.");
                var response = await _httpClient.PatchAsync("bookings/booking/update/", content);
                _logger.LogInformation($"Booking updated at time {DateTime.Now}.");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Booking {booking.BookingNumber} successfully updated at time {DateTime.Now}.");
                    TempData["SuccessMessage"] = $"Booking {booking.BookingNumber} successfully updated!";
                    return RedirectToAction("Index");
                }
                else
                 {
                    _logger.LogError($"Unexpected error TEST HALLO {DateTime.Now}.");
                    ViewData["ErrorMessage"] = "Do you try to use a new email? Forget it, Magda had a totally wrong idea about what to do in this case. Try not doing it.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when updating a booking at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when updating a booking at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }
            return View(booking);

        }

        [Authorize]
        // admin
        public async Task<IActionResult> RemoveBooking(string bookingNumber)
        {
            try
            {
                var token = HttpContext.Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetStringAsync($"bookings/booking/{bookingNumber}/simple");
                var booking = JsonSerializer.Deserialize<BookingVM>(response, _options);

                return View(booking);
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
        }

        [Authorize]
        // admin
        [HttpPost]
        public async Task<IActionResult> RemoveBookingConfirmed(string bookingNumber)
        {
            try
            {
                var token = HttpContext.Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.DeleteAsync($"bookings/booking/{bookingNumber}/delete");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Booking nr {bookingNumber} successfully deleted!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            }

            return RedirectToAction("Index");

        }
    }
}
