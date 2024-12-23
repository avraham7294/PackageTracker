using PackageTracker.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PackageTracker.Services
{
    public class PackageTrackingService
    {
        private readonly HttpClient _httpClient;

        public PackageTrackingService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MockPackageTrackingApi");
        }

        public async Task<PackageTracking?> GetPackageDetailsAsync(string trackingNumber)
        {
            try
            {
                var package = await _httpClient.GetFromJsonAsync<PackageTracking>($"{trackingNumber}");
                return package;
            }
            catch (HttpRequestException)
            {
                // Handle network errors
                return null;
            }
        }
    }
}
