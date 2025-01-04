using Microsoft.AspNetCore.Mvc;
using PackageTracker.Data;
using PackageTracker.Models;
using PackageTracker.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PackageTracker.Controllers
{
    public class PackageTrackingController : Controller
    {
        private readonly PackageTrackerContext _dbContext;
        private readonly PackageTrackingService _packageTrackingService;

        // Constructor to initialize services and database context
        public PackageTrackingController(PackageTrackingService packageTrackingService, PackageTrackerContext dbContext)
        {
            _packageTrackingService = packageTrackingService;
            _dbContext = dbContext;
        }

        [HttpGet]
        // Handles GET request to render the Index view
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        // Handles POST request to track a package by its tracking number
        public async Task<IActionResult> TrackPackage(string trackingNumber)
        {
            // Validate tracking number format
            if (string.IsNullOrEmpty(trackingNumber) ||
                !Regex.IsMatch(trackingNumber, "^[a-zA-Z0-9]{7,30}$"))
            {
                ViewBag.ErrorMessage = "Invalid tracking number. It must be 7-30 characters and contain only letters and numbers.";
                return View("Index");
            }

            // Fetch package details from external API using the service, Adding/Updating our database.
            var packageDetails = await _packageTrackingService.GetPackageDetailsAsync(trackingNumber); 
            if (packageDetails == null)
            {
                ViewBag.ErrorMessage = "Tracking number not found.";
                return View("Index");
            }

            // Fetch weather details
            var weatherWarning = await _packageTrackingService.CheckWeatherAsync(packageDetails.Destination);
            if (weatherWarning != null)
            {
                ViewBag.WeatherWarning = weatherWarning;
            }

            // Calculate average shipping time and similar shipments count
            var (averageShippingTime, shipmentCount) = await _packageTrackingService.GetAverageShippingTimeAsync(
            packageDetails.Origin,
            packageDetails.Destination,
            packageDetails.Carrier);

            // If average shipping time and count are available, calculate estimated arrival details
            if (averageShippingTime.HasValue)
            {
                var estimatedArrivalDate = packageDetails.ShippingDate.AddDays(averageShippingTime.Value);

                ViewBag.AverageShippingTime = averageShippingTime.Value.ToString("0.##");
                ViewBag.ShipmentCount = shipmentCount;
                ViewBag.DaysPassed = (DateTime.Now - packageDetails.ShippingDate).TotalDays.ToString("0");
                ViewBag.EstimatedArrivalDate = estimatedArrivalDate;
                ViewBag.EstimatedArrival = estimatedArrivalDate > DateTime.Now
                    ? estimatedArrivalDate.ToString("MMMM dd, yyyy")
                    : "it should have already arrived.";
            }
            else
            {
                ViewBag.AverageShippingTime = "No data available";
                ViewBag.ShipmentCount = 0;
            }

            return View("Details", packageDetails);
        }

        [HttpGet]
        public IActionResult ShippingStatistics()
        {
            // Fetch distinct origins and destinations for the dropdowns
            var origins = _dbContext.ShippingStatistics.Select(s => s.Origin).Distinct().ToList();
            var destinations = _dbContext.ShippingStatistics.Select(s => s.Destination).Distinct().ToList();

            // Pass origins and destinations to the ViewBag
            ViewBag.Origins = origins;
            ViewBag.Destinations = destinations;

            return View();
        }

        [HttpPost]
        public IActionResult FetchShippingStatistics(string origin, string destination)
        {
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                ViewBag.ErrorMessage = "Both origin and destination must be selected.";
                return RedirectToAction("ShippingStatistics");
            }

            // Query the database for matching records
            var statistics = _dbContext.ShippingStatistics
                .Where(s => s.Origin == origin && s.Destination == destination)
                .ToList();

            return PartialView("_ShippingStatisticsResults", statistics);
        }
    }
}
