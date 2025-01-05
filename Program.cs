using Microsoft.EntityFrameworkCore;
using PackageTracker.Data;
using PackageTracker.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // For MVC Controllers
builder.Services.AddControllers(); // For API Controllers


// Configure the database context
builder.Services.AddDbContext<PackageTrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HTTP Client for Mock Package Tracking API
builder.Services.AddHttpClient("MockPackageTrackingApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7289/api/PackageTracking/");
});

// Configure HTTP Client for Weather API
builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("http://api.weatherapi.com/v1/");
});


// Register custom services as Scoped
builder.Services.AddScoped<PackageTrackingService>();


// Add Swagger services for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Package Tracker API",
        Version = "v1",
        Description = "This API provides endpoints to retrieve package details and shipping statistics.  \n" + 
                      "It enables users to track package shipments by providing tracking numbers and fetch shipping performance metrics based on origin and destination.  \n" +
                      "The API integrates with external services to fetch real-time data, maintains a database for caching package and shipping details,  \n" + 
                      "and offers insights like average shipping times and weather-related warnings to improve delivery estimates.  \n",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        }
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Use detailed exception page in development
    app.UseSwagger(); // Enable Swagger in development
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Package Tracker API v1");
        options.RoutePrefix = ""; // Swagger UI available at /api-docs
    });
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Use custom error page in production
    app.UseHsts(); // Add HSTS headers for security
}

app.UseHttpsRedirection(); // Enforce HTTPS
app.UseStaticFiles(); // Serve static files like CSS, JS, and images

app.UseRouting();

app.UseAuthorization(); // Apply authorization middleware

// Define default route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map API Controllers
app.MapControllers(); // Map all [ApiController]-decorated controllers


app.Run();
