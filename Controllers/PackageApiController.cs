using Microsoft.AspNetCore.Mvc;
using PackageTracker.Data;
using PackageTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Controllers
{
    /// <summary>
    /// API Controller for managing package details and shipping statistics.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PackageApiController : ControllerBase
    {
        private readonly PackageTrackerContext _dbContext;

        public PackageApiController(PackageTrackerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves package details for a specific tracking number.
        /// </summary>
        /// <param name="trackingNumber">The tracking number of the package.</param>
        /// <returns>Package details or a not-found response.</returns>
        [HttpGet("Details/{trackingNumber}")]
        public async Task<IActionResult> GetPackageDetails(string trackingNumber)
        {
            var package = await _dbContext.PackageTrackings.FindAsync(trackingNumber);

            if (package == null)
            {
                return NotFound(new { Message = "Package not found." });
            }

            return Ok(package);
        }

        /// <summary>
        /// Retrieves shipping statistics for a specific origin and destination.
        /// </summary>
        /// <param name="origin">The origin of the shipments.</param>
        /// <param name="destination">The destination of the shipments.</param>
        /// <returns>Shipping statistics or a not-found response.</returns>
        [HttpGet("Statistics")]
        public IActionResult GetShippingStatistics(string origin, string destination)
        {
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                return BadRequest(new { Message = "Both origin and destination are required." });
            }

            var statistics = _dbContext.ShippingStatistics
                .Where(s => s.Origin == origin && s.Destination == destination)
                .ToList();

            if (!statistics.Any())
            {
                return NotFound(new { Message = "No statistics available for the specified route." });
            }

            return Ok(statistics);
        }
    }

}
