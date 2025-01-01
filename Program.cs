using Microsoft.EntityFrameworkCore;
using PackageTracker.Data;
using PackageTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Configure the database context
builder.Services.AddDbContext<PackageTrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HTTP Client for Mock API
builder.Services.AddHttpClient("MockPackageTrackingApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7289/api/PackageTracking/");
});

// Base URL for the Weather API
builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("http://api.weatherapi.com/v1/");
});


// Register services as Scoped
builder.Services.AddScoped<PackageTrackingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Use detailed exception page in development
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

app.Run();
