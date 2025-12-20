using Microsoft.AspNetCore.Http;

namespace MielShop.API.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string publicId);
        string GetImageUrl(string publicId);
    }
}