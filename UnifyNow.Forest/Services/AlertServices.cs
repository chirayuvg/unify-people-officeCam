using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Services
{
    public class AlertServices
    {
        public async Task<List<Alert>> GetDummyAlertsAsync()
        {
            List<Alert> alerts = new();
            Random random = new();

            string baseImagePath = "wwwroot/images";
            string projectDirectory = Directory.GetCurrentDirectory();
            string[] imageFiles = Directory.GetFiles(Path.Combine(projectDirectory, "wwwroot/images"));

            string[] aoiLocations = { "North Zone", "South Zone", "East Zone", "West Zone" };
            Severity[] severities = { Severity.Critical, Severity.High, Severity.Medium, Severity.Low };

            for (int i = 0; i < imageFiles.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(imageFiles[i]); // Extract the animal name from filename
                string fileUrl = $"{baseImagePath}/{Path.GetFileName(imageFiles[i])}"; // Full file path

                alerts.Add(new Alert
                {
                    Id = Guid.NewGuid(),
                    Name = $"Alert {i + 1}",
                    Severity = severities[random.Next(severities.Length)],
                    Animal = fileName, // Use the filename as the animal name
                    AOI = aoiLocations[random.Next(aoiLocations.Length)],
                    FileURL = fileUrl, // Use the correct file URL
                    TeamId = Guid.NewGuid(),
                    Team = new Team { Id = Guid.NewGuid(), Name = $"Team {random.Next(1, 4)}" }
                });
            }

            return await Task.FromResult(alerts);
        }
    }
}
