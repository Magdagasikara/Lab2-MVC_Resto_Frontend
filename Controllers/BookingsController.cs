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
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public BookingsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
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
        public async Task<IActionResult> UpdateBooking()
        {

            return View();
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
