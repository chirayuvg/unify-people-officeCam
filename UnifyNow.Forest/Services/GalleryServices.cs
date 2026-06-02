using Microsoft.EntityFrameworkCore;
using UnifyNow.Forest.CoreServices;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Services
{
    public class GalleryServices
    {
        private readonly ForestDb _forestDb;
        public GalleryServices(IDbContextFactory<ForestDb> dbContextFactory)
        {
            _forestDb = dbContextFactory.CreateDbContext();
        }

        public async Task<List<AnimalLogModel>> GetDummyResponsesAsync()
        {
            List<AnimalLogModel> objectResponses = new();

            // Define the correct image path
            string baseImagePath = "/GalleryAPI/Download?path=/documents/inference_images"; // Adjust this if needed for API serving
            string projectDirectory = Directory.GetCurrentDirectory();
            string[] animalImages = Directory.GetFiles(Path.Combine(projectDirectory, "wwwroot/images"));
            var images = _forestDb.ForestImages.ToList();
            // Dummy Data Generator
            Random random = new();
            string[] animals = { "Elephant", "Tiger", "Lion", "Zebra", "Giraffe" };
            string[] cameras = { "Camera 1", "Camera 2", "Camera 3" };
            string[] locations = { "North Zone", "South Zone", "East Zone", "West Zone" };
            string[] category = { "alert", "animal" };

            foreach(var image in images) {

                AnimalLogModel objectResponse = new()
                {
                    Id = image.Id, // Unique ID
                    Src = baseImagePath + "/" + image.ImageUrl,
                    Title = image.Id.ToString(), // Use filename as Title
                    Category = category[random.Next(category.Length)], // Static Category
                    Camera = cameras[random.Next(cameras.Length)], // Random Camera Name
                    Location = locations[random.Next(locations.Length)], // Random Location
                    DateTime = image.CreatedDate // Formatted timestamp
                };

                objectResponses.Add(objectResponse);
            }

            return await Task.FromResult(objectResponses);
        }


        public class AnimalLogModel
        {
            public Guid Id { get; set; }
            public string Src { get; set; }
            public string Title { get; set; }
            public string Category { get; set; }
            public string Camera { get; set; }
            public string Location { get; set; }
            public DateTime DateTime { get; set; }
        }

    }
}
