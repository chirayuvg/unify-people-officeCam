namespace Unify.People
{
    public class LocalStorageServices : IStorageServices
    {
        private IHostEnvironment HostingEnvironment { get; set; }
        private ILogger<LocalStorageServices> Logger { get; set; }
        private LocalStorageSettings _localStorageSettings;

        public LocalStorageServices(IHostEnvironment hostingEnvironment, ILogger<LocalStorageServices> logger, IConfiguration configration, LocalStorageSettings localStorageSettings)
        {
            HostingEnvironment = hostingEnvironment;
            Logger = logger;
            _localStorageSettings = localStorageSettings;
            configration.GetSection(nameof(LocalStorageSettings)).Bind(_localStorageSettings);
        }


        public async Task SaveFileAsync(IFormFile sourceFile, string targetPath, string fileName = null)
        {
            try
            {
                //targetPath = targetPath.Replace('\\', Path.DirectorySeparatorChar);
                if (fileName == null)
                    fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(sourceFile.FileName)}";
                string targetDirectory = Path.Combine(_localStorageSettings.FileRootFolder, targetPath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);

                }
                var targetPhysicalFile = Path.Combine(_localStorageSettings.FileRootFolder, targetPath, fileName);
                if (File.Exists(targetPhysicalFile))
                {
                    File.Delete(targetPhysicalFile);
                }

                var content = sourceFile.OpenReadStream();
                //targetPhysicalFile = "C:\\Documents\\mediadata\\training_models\\" + guid;
                var fileStream = new FileStream(targetPhysicalFile, FileMode.Create, FileAccess.Write);
                await content.CopyToAsync(fileStream);
                fileStream.Dispose();
                Console.WriteLine(targetPhysicalFile);

            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public async Task SaveImageFileAsync(IFormFile sourceFile, string targetFilePath)
        {
            targetFilePath = targetFilePath.Replace('\\', Path.DirectorySeparatorChar);
            using (var stream = new FileStream(targetFilePath, FileMode.Create))
            {
                await sourceFile.CopyToAsync(stream);
            }
        }


        public async Task CopyFileAsync(string sourceFile, string targetFile)
        {
            string targetPath = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            var sourcePath = Path.Combine(_localStorageSettings.FileRootFolder, sourceFile);
            targetPath = Path.Combine(_localStorageSettings.FileRootFolder, targetFile);

            File.Copy(sourcePath, targetPath);
        }

        public async Task DeleteFileAsync(string file)
        {
            var targetPath = Path.Combine(_localStorageSettings.FileRootFolder, file);
            if (System.IO.File.Exists(targetPath))
            {
                FileInfo fi = new FileInfo(targetPath);
                await Task.Factory.StartNew(() => fi.Delete());
            }
        }


        public async Task<bool> FileExistAsync(string file)
        {
            bool result = false;
            var targetFile = Path.Combine(_localStorageSettings.FileRootFolder, file);
            if (System.IO.File.Exists(targetFile))
            {
                FileInfo fi = new FileInfo(targetFile);
                result = await Task.Factory.StartNew(() => fi.Exists);
            }
            return result;
        }

        public async Task<Stream> GetFileAsync(string file)
        {
            var targetFile = Path.Combine(_localStorageSettings.FileRootFolder, file);
            if (System.IO.File.Exists(targetFile))
            {
                return File.OpenRead(targetFile);
            }
            return null;
        }

        public async Task<FileStream> GetFile(string file)
        {
            if (System.IO.File.Exists(file))
            {
                return File.OpenRead(file);
            }
            return null;
        }

        public async Task CreateDirectory(string directoryName)
        {
            var targetDirectory = Path.Combine(_localStorageSettings.FileRootFolder, directoryName);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            Console.WriteLine("Path "+ targetDirectory);
        }

        public async Task DeleteDirectory(string directoryName)
        {
            var targetDirectory = Path.Combine(_localStorageSettings.FileRootFolder, directoryName);
            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(targetDirectory, true);
            }
        }

        public async Task CopyDirectory(string source, string destination)
        {
            var dir = new DirectoryInfo(source);
            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destination);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destination, file.Name);
                file.CopyTo(targetFilePath);
            }
        }

    }

    public class LocalStorageSettings
    {
        // public string FileRootFolder { get; set; } = "C:\\documents\\visionstudio";
        //public string FileRootFolder { get; set; } = "/documents/visionstudio";
        // production server working
        //public string FileRootFolder { get; set; } = "/documents/visionstudio";
        public string TrainingModelRootFolder { get; set; } = "/documents/visionstudio/training_models";
        public string WorkPlaceModelRootFolder { get; set; } = "/documents/workplace/model";

        //public string FileRootFolder { get; set; } = "c:\\documents";
        public string FileRootFolder { get; set; } = "/documents";
        public string InferenceRootFolder { get; set; } = "/documents";

    }
}
