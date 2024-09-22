using Lab2_MVC_Resto_Frontend.Models;
using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
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
        public async Task<IActionResult> Index()
        {
            // Get bookings from API
            var responseBookings = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}bookings");
            var bookings = JsonSerializer.Deserialize<IEnumerable<BookingWithTablesEndTimeDto>>(responseBookings, _options);

            // Show bookings from now and one week ahead
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);

            // Get restaurant's tables from API
            var responseTables = await _httpClient.GetStringAsync($"{_httpClient.BaseAddress}tables");
            var tables = JsonSerializer.Deserialize<List<TableVM>>(responseTables, _options);

            foreach (var table in tables)
            {
                table.Bookings = bookings
                        .Where(b => b.Tables != null && b.Tables.Any(t => t.TableNumber == table.TableNumber))
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
            var schedule = new ScheduleVM
            {
                Tables = tables,
                StartDate = startDate,
                EndDate = endDate
            };

            return View(schedule);
        }
        // admin
        public async Task<IActionResult> AddBooking()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> AddBooking(BookingAddVM booking)
        {

            if (!ModelState.IsValid)
            {
                return View(booking);
            }
            var json = JsonSerializer.Serialize(booking);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUri}menu/bookings/booking/add", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Reservation for {booking.AmountOfGuests} on {booking.ReservationStart} successfully created. We sent a confirmation mail to {booking.Email}.";
                return RedirectToAction("Index");
            }
            else
            {
                ViewData["ErrorMessage"] = "Something went wrong. Feel free to try again.";
                return View(booking);
            }
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
