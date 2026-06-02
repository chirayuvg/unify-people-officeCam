namespace UnifyNow.Forest.CoreServices
{
    public interface IStorageServices
    {
        Task CopyFileAsync(string sourceFile, string targetFile);
        Task CreateDirectory(string directoryName);
        Task CopyDirectory(string source, string target);
        Task<List<string>> DirectoryFileNames(string source);
        Task DeleteDirectory(string directoryName);
        Task DeleteFileAsync(string file);
        Task<bool> FileExistAsync(string file);
        Task<Stream> GetFileAsync(string file);
        Task<FileStream> GetFile(string file);
        Task SaveFileAsync(IFormFile sourceFile, string targetFile);

        Task UploadFile(IFormFile sourceFile, string targetPath, string fileName = null);
        Task SaveImageFileAsync(IFormFile sourceFile, string targetFilePath);
    }
}
