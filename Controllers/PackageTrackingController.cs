using Microsoft.AspNetCore.Mvc;
using PackageTracker.Data;
using PackageTracker.Models;
using PackageTracker.Services;
using System.Threading.Tasks;

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

            // Calculate the average shipping time, call directly to the context
            var averageShippingTime = _dbContext.GetAverageShippingTime(packageDetails.Origin, packageDetails.Destination);

            ViewBag.AverageShippingTime = averageShippingTime;

            return View("Details", packageDetails);
        }
    }
}
