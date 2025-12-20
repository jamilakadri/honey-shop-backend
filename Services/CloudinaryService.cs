using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MielShop.API.Models;

namespace MielShop.API.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            // Get settings from environment variables (Railway) or appsettings
            var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
                ?? config.Value.CloudName;
            var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
                ?? config.Value.ApiKey;
            var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
                ?? config.Value.ApiSecret;

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type {extension} is not allowed");
            }

            // Upload to Cloudinary
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder, // e.g., "miel-shop/products" or "miel-shop/categories"
                Transformation = new Transformation()
                    .Width(1000)
                    .Height(1000)
                    .Crop("limit")
                    .Quality("auto")
                    .FetchFormat("auto") // Auto-convert to WebP when supported
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            // Return the secure URL (HTTPS)
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false;
            }

            try
            {
                // Extract public ID from Cloudinary URL
                var publicId = GetPublicIdFromUrl(imageUrl);

                if (string.IsNullOrEmpty(publicId))
                {
                    return false;
                }

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }

        public string GetImageUrl(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return string.Empty;
            }

            return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
        }

        private string GetPublicIdFromUrl(string imageUrl)
        {
            // Example URL: https://res.cloudinary.com/cloud_name/image/upload/v1234567890/folder/image.jpg
            // Extract: folder/image (without extension)

            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
            {
                return string.Empty;
            }

            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');

                // Find the upload segment
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
                {
                    return string.Empty;
                }

                // Get everything after version (v1234567890)
                var publicIdParts = segments.Skip(uploadIndex + 2).ToList();
                var publicId = string.Join("/", publicIdParts);

                // Remove file extension
                var lastDot = publicId.LastIndexOf('.');
                if (lastDot > 0)
                {
                    publicId = publicId.Substring(0, lastDot);
                }

                return publicId;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}