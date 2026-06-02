namespace Unify.People

{
    public interface IStorageServices
    {
        Task CopyFileAsync(string sourceFile, string targetFile);
        Task CreateDirectory(string directoryName);
        Task DeleteDirectory(string directoryName);

        Task CopyDirectory(string source, string target);
        Task DeleteFileAsync(string file);
        Task<bool> FileExistAsync(string file);
        Task<Stream> GetFileAsync(string file);
        Task<FileStream> GetFile(string file);
        //Task SaveFileAsync(IFormFile sourceFile, string targetFile);

        Task SaveFileAsync(IFormFile sourceFile, string targetPath, string fileName = null);
        Task SaveImageFileAsync(IFormFile sourceFile, string targetFilePath);
    }
}
