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

        // Constructor to initialize HTTP client, database context, and logger
        public PackageTrackingService(IHttpClientFactory httpClientFactory,
                                       PackageTrackerContext dbContext,
                                       ILogger<PackageTrackingService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("MockPackageTrackingApi");
            _dbContext = dbContext;
            _logger = logger;
        }

        // Fetches package details from external API, Add/Update our database
        public async Task<PackageTracking?> GetPackageDetailsAsync(string trackingNumber)
        {
            try
            {
                // Step 1: Fetch package from external API
                var package = await FetchPackageFromApiAsync(trackingNumber);
                if (package == null) return null;

                // Step 2: Check if the package exists in our database
                var existingPackage = await GetPackageFromDatabaseAsync(trackingNumber);

                // Step 3: Add or update the package in our database
                await AddOrUpdatePackageInDatabaseAsync(package, existingPackage);

                return package;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while processing tracking number {TrackingNumber}", trackingNumber);
                return null;
            }
        }


        // Fetches package details from the external API
        private async Task<PackageTracking?> FetchPackageFromApiAsync(string trackingNumber)
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

        // Retrieves a package from our database based on tracking number
        private async Task<PackageTracking?> GetPackageFromDatabaseAsync(string trackingNumber)  // Check If Package Exists in our Database
        {
            return await _dbContext.PackageTrackings
                .FirstOrDefaultAsync(p => p.Id == trackingNumber);
        }

        // Adds a new package or updates an existing one in our database
        private async Task AddOrUpdatePackageInDatabaseAsync(PackageTracking package, PackageTracking? existingPackage) // Add or Update Package in the Database
        {
            if (existingPackage == null)
            {
                // Add new package
                await _dbContext.PackageTrackings.AddAsync(package);
            }
            else
            {
                existingPackage.Status = package.Status;
                existingPackage.DeliveryDate = package.DeliveryDate;
            }

            await _dbContext.SaveChangesAsync();
        }

        // Method to Fetch Shipping Statistics
        public async Task<ShippingStatistics?> GetShippingStatisticsAsync(string origin, string destination, string carrier)
        {
            return await _dbContext.ShippingStatistics
                .FirstOrDefaultAsync(stat => stat.Origin == origin && 
                                             stat.Destination == destination && 
                                             stat.Carrier == carrier);
        }

        // Calculates average shipping time and count for similar shipments
        // And updating the cached ShippingStatistics table
        public async Task UpdateShippingStatisticsAsync(string origin, string destination, string carrier)
        {
            var cutoffDate = DateTime.Now.AddDays(-60); // Limit to shipments in the last 60 days

            var relevantPackages = await _dbContext.PackageTrackings
                .Where(p => p.Origin == origin && 
                            p.Destination == destination && 
                            p.Carrier == carrier && 
                            p.Status == "Completed" && 
                            p.ShippingDate >= cutoffDate)
                .ToListAsync();

            if (!relevantPackages.Any()) return;

            //  Check for DeliveryDate being non-null before performing calculations.
            var totalDays = relevantPackages
                .Sum(p => (p.DeliveryDate.HasValue ? (p.DeliveryDate.Value - p.ShippingDate).TotalDays : 0)); 

            var averageDays = totalDays / relevantPackages.Count;

            var statistics = await GetShippingStatisticsAsync(origin, destination, carrier);

            if (statistics == null) // If this kind is not existing at all in the cache table
            {
                statistics = new ShippingStatistics
                {
                    Origin = origin,
                    Destination = destination,
                    Carrier = carrier,
                    AverageShippingTime = averageDays,
                    ShipmentCount = relevantPackages.Count,
                    LastUpdateDate = DateTime.Now
                };
                await _dbContext.ShippingStatistics.AddAsync(statistics);
            }
            else // If exist, but outdated.
            {
                statistics.AverageShippingTime = averageDays;
                statistics.ShipmentCount = relevantPackages.Count;
                statistics.LastUpdateDate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }


        // Calculates average shipping time
        public async Task<(double? AverageDays, int Count)> GetAverageShippingTimeAsync(string origin, string destination, string carrier)
        {
           // First check the cached ShippingStatistics table
            var statistics = await GetShippingStatisticsAsync(origin, destination, carrier); 

            if (statistics != null && statistics.LastUpdateDate >= DateTime.Now.AddDays(-3))
            {
                return (statistics.AverageShippingTime, statistics.ShipmentCount);
            }

            // Update cached statistics table if not found or outdated
            await UpdateShippingStatisticsAsync(origin, destination, carrier);

            statistics = await GetShippingStatisticsAsync(origin, destination, carrier);
            return statistics != null
                ? (statistics.AverageShippingTime, statistics.ShipmentCount)
                : (null, 0);
        }

        // Fetch weather data real API
        public async Task<string?> CheckWeatherAsync(string destination)
        {
            try
            {
                var apiKey = "6d4213dc4fa04aea949150239250101";
                var response = await _httpClient.GetFromJsonAsync<WeatherResponse>($"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={destination}");
                if (response?.Current?.TempC != null)
                {
                    var temp = response.Current.TempC;
                    if (temp < 10 || temp > 20) // Define extreme weather conditions
                    {
                        return "Due to extreme weather in the area, additional delays may occur.";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while fetching weather data for {Destination}", destination);
            }
            return null;
        }

    }
}
