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

        public PackageTrackingController(PackageTrackingService packageTrackingService, PackageTrackerContext dbContext)
        {
            _packageTrackingService = packageTrackingService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TrackPackage(string trackingNumber)
        {
            // Validate tracking number format
            if (string.IsNullOrEmpty(trackingNumber) ||
                !Regex.IsMatch(trackingNumber, "^[a-zA-Z0-9]{7,30}$"))
            {
                ViewBag.ErrorMessage = "Invalid tracking number. It must be 7-30 characters and contain only letters and numbers.";
                return View("Index");
            }

            // Fetch package from the external API > Check if the package exists in our database > Add or update the package in our database
            var packageDetails = await _packageTrackingService.GetPackageDetailsAsync(trackingNumber); 
            if (packageDetails == null)
            {
                ViewBag.ErrorMessage = "Tracking number not found.";
                return View("Index");
            }

            // Await the average shipping time task
            var (averageShippingTime, shipmentCount) = await _packageTrackingService.GetAverageShippingTimeAsync(packageDetails.Origin, packageDetails.Destination);

            if (averageShippingTime.HasValue)
            {
                var daysPassed = (DateTime.Now - packageDetails.ShippingDate).TotalDays;
                var estimatedArrival = averageShippingTime.Value - daysPassed;

                ViewBag.AverageShippingTime = averageShippingTime.Value.ToString("0.##");
                ViewBag.ShipmentCount = shipmentCount;
                ViewBag.DaysPassed = daysPassed.ToString("0");
                ViewBag.EstimatedArrival = estimatedArrival > 0
                    ? $"approximately {Math.Ceiling(estimatedArrival)} days"
                    : "it should have already arrived.";
            }
            else
            {
                ViewBag.AverageShippingTime = "No data available";
                ViewBag.ShipmentCount = 0;
            }

            return View("Details", packageDetails);
        }
    }
}
