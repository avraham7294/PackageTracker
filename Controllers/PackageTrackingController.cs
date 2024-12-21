using Microsoft.AspNetCore.Mvc;
using PackageTracker.Data;
using PackageTracker.Models;
using System.Threading.Tasks;

namespace PackageTracker.Controllers
{
    public class PackageTrackingController : Controller
    {
        private readonly PackageTrackerContext _context;
        public PackageTrackingController(PackageTrackerContext context)
        {
            _context = context;
        }

        // GET: /PackageTracking/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: /PackageTracking/Track
        [HttpPost]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            if (string.IsNullOrEmpty(trackingNumber))
            {
                ModelState.AddModelError(string.Empty, "Please enter a tracking number.");
                return View("Index");
            }

            // Mock data until API is integrated
            var mockPackage = new PackageTracking
            {
                TrackingNumber = trackingNumber,
                Carrier = "MockCarrier",
                Status = "In Transit",
                ShippingDate = System.DateTime.Now.AddDays(-3),
                DeliveryDate = System.DateTime.Now.AddDays(2),
                Origin = "New York, NY",
                Destination = "San Francisco, CA"
            };

            // Optionally, save this package in the database (mock for now)
            await _context.Packages.AddAsync(mockPackage);
            await _context.SaveChangesAsync();

            return View("Details", mockPackage);
        }


    }
}
