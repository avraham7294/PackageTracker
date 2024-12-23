using Microsoft.AspNetCore.Mvc;
using PackageTracker.Models;
using PackageTracker.Services;
using System.Threading.Tasks;

namespace PackageTracker.Controllers
{
    public class PackageTrackingController : Controller
    {
        private readonly PackageTrackingService _packageTrackingService;

        public PackageTrackingController(PackageTrackingService packageTrackingService)
        {
            _packageTrackingService = packageTrackingService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TrackPackage(string trackingNumber)
        {
            if (string.IsNullOrEmpty(trackingNumber))
            {
                ViewBag.ErrorMessage = "Please provide a tracking number.";
                return View("Index");
            }

            var packageDetails = await _packageTrackingService.GetPackageDetailsAsync(trackingNumber);

            if (packageDetails == null)
            {
                ViewBag.ErrorMessage = "Tracking number not found.";
                return View("Index");
            }

            // Calculate the average shipping time
            var averageShippingTime = _packageTrackingService
                .GetAverageShippingTime(packageDetails.Origin, packageDetails.Destination);

            ViewBag.AverageShippingTime = averageShippingTime;

            return View("Details", packageDetails);
        }
    }
}
