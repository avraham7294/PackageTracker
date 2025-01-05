# Package Tracker Application

## Overview
The Package Tracker is a web application designed to provide real-time package tracking and shipping statistics. The application integrates with external APIs to fetch package details and weather updates, offering users a seamless experience for tracking shipments and analyzing shipping patterns.

---

## Features

1. **Package Tracking:**
   - Users can input a tracking number to retrieve package details such as status, carrier, origin, destination, and shipping date.
   - Displays weather warnings for the destination if extreme conditions are detected.
   - Calculates average shipping time and estimated delivery date based on historical data.

2. **Shipping Statistics:**
   - Provides insights into shipping patterns, including average shipping time and shipment counts for specific routes.
   - Allows users to filter statistics by origin and destination.

3. **Weather Integration:**
   - Fetches current weather conditions for the package's destination to anticipate potential delays.

---

## Technologies Used

- **Backend:** ASP.NET Core
- **Frontend:** Razor Views (HTML, CSS)
- **Database:** Entity Framework Core with SQLite
- **APIs:**
  - Mock Package Tracking API (HTTP Client)
  - Weather API
- **Libraries:**
  - LINQ for querying data
  - Regex for input validation

---

## Project Structure

### Controllers
- **`PackageApiController.cs`**: Handles API endpoints for retrieving package details and shipping statistics.
- **`PackageTrackingController.cs`**: Manages user interactions for tracking packages and viewing statistics.

### Models
- **`PackageTracking.cs`**: Represents package data such as tracking number, carrier, status, and shipping dates.
- **`ShippingStatistics.cs`**: Represents cached shipping data for specific routes.
- **`WeatherResponse.cs`**: Models the response structure from the Weather API.

### Services
- **`PackageTrackingService.cs`**: Implements business logic for:
  - Fetching package details from external APIs.
  - Updating and querying the database.
  - Fetching and analyzing shipping statistics.
  - Integrating weather data.

### Data
- **`PackageTrackerContext.cs`**: Configures database tables for `PackageTrackings` and `ShippingStatistics`.

### Views
- **`Index.cshtml`**: Homepage for tracking packages.
- **`Details.cshtml`**: Displays detailed package information.
- **`ShippingStatistics.cshtml`**: Displays shipping statistics with filtering options.
- **`_ShippingStatisticsResults.cshtml`**: Partial view for rendering statistics results.

---

## Setup and Installation

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/) (Version 6.0 or higher recommended)
- SQLite (or any other database supported by Entity Framework Core)
- API keys for external services (e.g., Weather API)

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/avraham7294/package-tracker.git
   cd package-tracker
   ```
2. Configure the database:
   - Update the connection string in `appsettings.json`:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Data Source=PackageTracker.db"
       }
     }
     ```
   - Apply migrations:
     ```bash
     dotnet ef database update
     ```

3. Configure the Weather API:
   - Replace the placeholder API key in `PackageTrackingService.cs`:
     ```csharp
     var apiKey = "YOUR_API_KEY_HERE";
     ```

4. Run the application:
   ```bash
   dotnet run
   ```
   The application will be accessible at `http://localhost:5000`.

---

## Usage

### Tracking Packages
1. Navigate to the homepage (`/PackageTracking/Index`).
2. Enter a valid tracking number and submit.
3. View the package details, including:
   - Current status
   - Average shipping time
   - Estimated delivery date
   - Weather warnings (if applicable)

### Viewing Shipping Statistics
1. Navigate to the `Shipping Statistics` page (`/PackageTracking/ShippingStatistics`).
2. Select an origin and destination.
3. View detailed shipping statistics, including:
   - Average shipping time
   - Shipment count
   - Last updated date

---

## API Endpoints

### Package API Controller
- **Get Package Details**: `GET /api/PackageApi/Details/{trackingNumber}`
- **Get Shipping Statistics**: `GET /api/PackageApi/Statistics?origin={origin}&destination={destination}`

### Package Tracking Controller
- **Track Package**: `POST /PackageTracking/TrackPackage`
- **Fetch Shipping Statistics**: `POST /PackageTracking/FetchShippingStatistics`

---

## Contribution

1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit changes:
   ```bash
   git commit -m "Add feature description"
   ```
4. Push the branch and create a Pull Request.

---

## License
This project is licensed under the [MIT License](LICENSE).

---

## Contact
For questions or support, reach out to [avraham7294@gmail.com](mailto:avraham7294@gmail.com).

