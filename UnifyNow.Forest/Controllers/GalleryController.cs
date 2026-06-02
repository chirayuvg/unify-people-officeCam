using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;

namespace UnifyNow.Forest.Controllers
{
    public class GalleryController : PageController
    {
        private PopupView popupImage;
        public CardView<GalleryImage> cardView { get; set; } = new();

        public override void Init()
        {
            this.Page.Title = "Gallery";

            var tileView = this.AddTileView();
            tileView.OnLoadData += GalleryController_OnLoadData;
            tileView.OnSelect += TileView_OnSelect;

            popupImage = this.AddPopupView();
            popupImage.SetId("popupImage");
        }

        private void TileView_OnSelect(TileViewItem obj)
        {
            //Navigation.ShowPopup("", "GalleryPopup", "Index", new { ImageFolderPath = obj.ImageUrl });
            Navigation.ShowPage("Index", "GalleryPopup", new { ImageFolderPath = obj.ImageUrl });
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GalleryController_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            try
            {
                List<TileViewItem> tileViewItems = new();

                try
                {
                    string imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    // Get all image files from the folder
                    string[] imageFiles = Directory.GetFiles(imageFolderPath, "*.*") // Adjust the extension filter if needed
                                                   .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg"))
                                                   .ToArray();

                    foreach (var imagePath in imageFiles)
                    {
                        string fileName2 = Path.GetFileName(imagePath);
                        string publicUrl = $"/images/{fileName2}"; // Web-accessible URL

                        tileViewItems.Add(new TileViewItem
                        {
                            Id = Guid.NewGuid().ToString(), // Generate a unique ID
                            Title = Path.GetFileNameWithoutExtension(fileName2), // Use file name as title
                            ImageUrl = publicUrl // Store the image URL
                        });
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing image : {ex.Message}");
                }
                // }

                // 3️⃣ Apply DevExtreme data processing
                var loadResult = DevExtreme.AspNet.Data.DataSourceLoader.Load(tileViewItems, loadOptions);

                return loadResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                return new DevExtreme.AspNet.Data.ResponseModel.LoadResult { data = new List<TileViewItem>(), totalCount = 0 };
            }
        }
    }

    public class GalleryImage
    {
        public IFrameView FrameView { get; set; } = new();
    }
}
