using PackageTracker.Data;
using PackageTracker.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PackageTracker.Services
{
    public class PackageTrackingService
    {
        private readonly HttpClient _httpClient;
        private readonly PackageTrackerContext _dbContext;
        private readonly ILogger<PackageTrackingService> _logger;

        public PackageTrackingService(IHttpClientFactory httpClientFactory,
                                       PackageTrackerContext dbContext,
                                       ILogger<PackageTrackingService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("MockPackageTrackingApi");
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<PackageTracking?> GetPackageDetailsAsync(string trackingNumber)
        {
            try
            {
                // Step 1: Fetch package from the API
                var package = await FetchPackageFromApiAsync(trackingNumber);
                if (package == null) return null;

                // Step 2: Check if the package exists in the database
                var existingPackage = await GetPackageFromDatabaseAsync(trackingNumber);

                // Step 3: Add or update the package in the database
                await AddOrUpdatePackageInDatabaseAsync(package, existingPackage);

                return package;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while processing tracking number {TrackingNumber}", trackingNumber);
                return null;
            }
        }


        private async Task<PackageTracking?> FetchPackageFromApiAsync(string trackingNumber)  // Fetch Package Details from the API
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PackageTracking>($"{trackingNumber}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while fetching package details for {TrackingNumber} from the API", trackingNumber);
                return null;
            }
        } 

        private async Task<PackageTracking?> GetPackageFromDatabaseAsync(string trackingNumber)  // Check If Package Exists in Database
        {
            return await _dbContext.PackageTrackings
                .FirstOrDefaultAsync(p => p.Id == trackingNumber);
        }

        private async Task AddOrUpdatePackageInDatabaseAsync(PackageTracking package, PackageTracking? existingPackage) // Add or Update Package in the Database
        {
            if (existingPackage == null)
            {
                // Add new package
                await _dbContext.PackageTrackings.AddAsync(package);
            }
            else
            {
                // Update existing package details
                existingPackage.Carrier = package.Carrier;
                existingPackage.Status = package.Status;
                existingPackage.ShippingDate = package.ShippingDate;
                existingPackage.DeliveryDate = package.DeliveryDate;
                existingPackage.Origin = package.Origin;
                existingPackage.Destination = package.Destination;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<(double? AverageDays, int Count)> GetAverageShippingTimeAsync(string origin, string destination)
        {
            var cutoffDate = DateTime.Now.AddDays(-60); // 60 days ago

            var relevantPackages = await _dbContext.PackageTrackings
                .Where(p => p.Origin == origin && p.Destination == destination
                            && p.Status == "Completed"
                            && p.ShippingDate >= cutoffDate)
                .ToListAsync();

            if (!relevantPackages.Any()) return (null, 0);

            var totalDays = relevantPackages
                .Sum(p => (p.DeliveryDate - p.ShippingDate).TotalDays);

            var averageDays = totalDays / relevantPackages.Count;
            return (averageDays, relevantPackages.Count);
        }
    }
}
