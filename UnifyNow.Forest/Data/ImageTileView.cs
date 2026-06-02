using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Elastic.Clients.Elasticsearch.Xpack;

using UnifyNow.Forest.Data.Elasticsearch;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Data
{
    public class ImageTileView : TileView
    {
        public List<TileViewItem> Items { get; private set; } = new();

        public ImageTileView()
        {
            LoadDataAsync(new DataSourceLoadOptions()); // Call async method properly
        }

        private async void LoadDataAsync(DataSourceLoadOptions options)
        {
            try
            {
                var items = await FetchDataAsync(options);
                Items = items; // Assign result
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image data: {ex.Message}");
            }
        }

        public async Task<List<TileViewItem>> FetchDataAsync(DataSourceLoadOptions options)
        {
            var db = AppStudio.Framework.Core.CoreServices.GetService<ElasticDb>();
            List<TileViewItem> tileViewItems = new();

            try
            {
                string sql = "SELECT id, designation AS Title, faceImage, '' AS ImageUrl FROM attendanceindex";

                // 1️⃣ Fetch Data from ElasticSearch
                var searchResults = await db.GetAllAsync<Images>(sql);

                // Ensure searchResults is not null before proceeding
                if (searchResults == null || !searchResults.Any())
                {
                    Console.WriteLine("No data retrieved from ElasticSearch.");
                    return tileViewItems;
                }

                string saveDirectory = "C:\\Images"; // Local directory for storing images
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                // 2️⃣ Convert Base64 to Images and Save
                foreach (var item in searchResults)
                {
                    if (string.IsNullOrEmpty(item.faceImage))
                        continue;

                    string fileName = $"{Guid.NewGuid()}.png";
                    string filePath = Path.Combine(saveDirectory, fileName);

                    try
                    {
                        // Convert Base64 to byte array
                        byte[] imageBytes = Convert.FromBase64String(item.faceImage);

                        // Save image asynchronously
                        await File.WriteAllBytesAsync(filePath, imageBytes);

                        // Add new TileViewItem to the list
                        tileViewItems.Add(new TileViewItem
                        {
                            Id = item.id.ToString(),
                            Title = item.Title,
                            ImageUrl = filePath // Store the saved file path
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing image for {item.Title}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
            }

            return tileViewItems;
        }
    }

    }
