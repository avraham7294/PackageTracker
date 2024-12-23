using PackageTracker.Data;
using PackageTracker.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PackageTracker.Services
{
    public class PackageTrackingService
    {
        private readonly HttpClient _httpClient;
        private readonly PackageTrackerContext _dbContext;

        public PackageTrackingService(IHttpClientFactory httpClientFactory, PackageTrackerContext dbContext)
        {
            _httpClient = httpClientFactory.CreateClient("MockPackageTrackingApi");
            _dbContext = dbContext;
        }

        public async Task<PackageTracking?> GetPackageDetailsAsync(string trackingNumber)
        {
            try
            {
                // Fetch package details from the Mock API
                var package = await _httpClient.GetFromJsonAsync<PackageTracking>($"{trackingNumber}");
                if (package == null) return null;

                // Check if the tracking number already exists in the database
                var existingPackage = _dbContext.PackageTrackings
                    .FirstOrDefault(p => p.Id == trackingNumber);

                if (existingPackage == null)
                {
                    // Add new package
                    _dbContext.PackageTrackings.Add(package);
                }
                else
                {
                    // Update existing package
                    existingPackage.Carrier = package.Carrier;
                    existingPackage.Status = package.Status;
                    existingPackage.ShippingDate = package.ShippingDate;
                    existingPackage.DeliveryDate = package.DeliveryDate;
                    existingPackage.Origin = package.Origin;
                    existingPackage.Destination = package.Destination;
                }

                await _dbContext.SaveChangesAsync();
                return package;
            }
            catch (HttpRequestException)
            {
                return null; // Handle network errors
            }
        }

        public double? GetAverageShippingTime(string origin, string destination) // ***Thinking to move this to the PackageTrackerConetext
        {
            var relevantPackages = _dbContext.PackageTrackings
                .Where(p => p.Origin == origin && p.Destination == destination)
                .ToList();

            if (!relevantPackages.Any()) return null;

            var totalDays = relevantPackages
                .Where(p => p.DeliveryDate > p.ShippingDate)
                .Sum(p => (p.DeliveryDate - p.ShippingDate).TotalDays);

            return totalDays / relevantPackages.Count;
        }
    }
}
