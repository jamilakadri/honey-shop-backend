using MielShop.API.Models;
using MielShop.API.Repositories;

namespace MielShop.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _unitOfWork.Products.GetAllAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _unitOfWork.Products.GetActiveProductsAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
        {
            return await _unitOfWork.Products.GetFeaturedProductsAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _unitOfWork.Products.GetByIdAsync(productId);
        }

        public async Task<Product?> GetProductWithDetailsAsync(int productId)
        {
            return await _unitOfWork.Products.GetProductWithDetailsAsync(productId);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _unitOfWork.Products.SearchProductsAsync(searchTerm);
        }

        // ✅ NOUVELLE MÉTHODE: Générer un SKU unique automatiquement
        private async Task<string> GenerateUniqueSKUAsync(string productName)
        {
            // 1. Créer un préfixe basé sur le nom du produit
            var prefix = CreateSKUPrefix(productName);

            // 2. Obtenir tous les produits existants
            var allProducts = await _unitOfWork.Products.GetAllAsync();

            // 3. Trouver tous les SKUs qui commencent par ce préfixe
            var existingSKUs = allProducts
                .Where(p => !string.IsNullOrEmpty(p.SKU) && p.SKU.StartsWith(prefix))
                .Select(p => p.SKU)
                .ToList();

            // 4. Générer un nouveau numéro
            int counter = 1;
            string newSKU;

            do
            {
                newSKU = $"{prefix}-{counter:D3}"; // Format: PREFIX-001, PREFIX-002, etc.
                counter++;
            }
            while (existingSKUs.Contains(newSKU));

            return newSKU;
        }

        // ✅ NOUVELLE MÉTHODE: Créer un préfixe SKU à partir du nom
        private string CreateSKUPrefix(string productName)
        {
            // Nettoyer et normaliser le nom
            var cleanName = productName
                .ToUpperInvariant()
                .Trim()
                .Normalize(System.Text.NormalizationForm.FormD);

            // Supprimer les accents
            var stringBuilder = new System.Text.StringBuilder();
            foreach (var c in cleanName)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            cleanName = stringBuilder.ToString();

            // Remplacer les espaces et caractères spéciaux par des tirets
            cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"[^A-Z0-9]+", "-");

            // Limiter à 10 caractères maximum pour le préfixe
            if (cleanName.Length > 10)
            {
                cleanName = cleanName.Substring(0, 10);
            }

            // Supprimer les tirets au début et à la fin
            cleanName = cleanName.Trim('-');

            return cleanName;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            // ✅ NOUVEAU: Générer automatiquement le SKU s'il est vide
            if (string.IsNullOrWhiteSpace(product.SKU))
            {
                product.SKU = await GenerateUniqueSKUAsync(product.Name);
                Console.WriteLine($"✅ SKU auto-généré: {product.SKU}");
            }
            else
            {
                // Vérifier que le SKU fourni n'existe pas déjà
                var allProducts = await _unitOfWork.Products.GetAllAsync();
                if (allProducts.Any(p => p.SKU == product.SKU))
                {
                    throw new InvalidOperationException($"Le SKU '{product.SKU}' existe déjà. Veuillez en choisir un autre.");
                }
            }

            // ✅ FIXED: Ensure all DateTime properties are UTC
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            // Convert HarvestDate to UTC if it exists
            if (product.HarvestDate.HasValue)
            {
                product.HarvestDate = product.HarvestDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(product.HarvestDate.Value, DateTimeKind.Utc)
                    : product.HarvestDate.Value.ToUniversalTime();
            }

            // Convert ExpiryDate to UTC if it exists
            if (product.ExpiryDate.HasValue)
            {
                product.ExpiryDate = product.ExpiryDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(product.ExpiryDate.Value, DateTimeKind.Utc)
                    : product.ExpiryDate.Value.ToUniversalTime();
            }

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product;
        }

        public async Task<Product?> UpdateProductAsync(int productId, Product product)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(productId);
            if (existingProduct == null)
                return null;

            existingProduct.Name = product.Name;
            existingProduct.Slug = product.Slug;
            existingProduct.Description = product.Description;
            existingProduct.ShortDescription = product.ShortDescription;
            existingProduct.Price = product.Price;
            existingProduct.CompareAtPrice = product.CompareAtPrice;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.IsActive = product.IsActive;
            existingProduct.IsFeatured = product.IsFeatured;

            // ✅ NOUVEAU: Mettre à jour le SKU seulement si fourni et différent
            if (!string.IsNullOrWhiteSpace(product.SKU) && product.SKU != existingProduct.SKU)
            {
                // Vérifier que le nouveau SKU n'existe pas déjà
                var allProducts = await _unitOfWork.Products.GetAllAsync();
                if (allProducts.Any(p => p.ProductId != productId && p.SKU == product.SKU))
                {
                    throw new InvalidOperationException($"Le SKU '{product.SKU}' existe déjà. Veuillez en choisir un autre.");
                }
                existingProduct.SKU = product.SKU;
            }

            existingProduct.UpdatedAt = DateTime.UtcNow;

            // ✅ FIXED: Update HarvestDate and ExpiryDate with UTC conversion
            if (product.HarvestDate.HasValue)
            {
                existingProduct.HarvestDate = product.HarvestDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(product.HarvestDate.Value, DateTimeKind.Utc)
                    : product.HarvestDate.Value.ToUniversalTime();
            }

            if (product.ExpiryDate.HasValue)
            {
                existingProduct.ExpiryDate = product.ExpiryDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(product.ExpiryDate.Value, DateTimeKind.Utc)
                    : product.ExpiryDate.Value.ToUniversalTime();
            }

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();

            return existingProduct;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return false;

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateStockAsync(int productId, int newStock)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return false;

            product.StockQuantity = newStock;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<Product?> GetProductBySlugAsync(string slug)
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return products.FirstOrDefault(p => p.Slug == slug);
        }

        public async Task<IEnumerable<ProductImage>> GetProductImagesAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetProductWithDetailsAsync(productId);
            return product?.ProductImages ?? new List<ProductImage>();
        }

        public async Task<ProductImage> AddProductImageAsync(ProductImage image)
        {
            image.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.ProductImages.AddAsync(image);
            await _unitOfWork.SaveChangesAsync();
            return image;
        }

        public async Task<ProductImage?> UpdateProductImageAsync(ProductImage image)
        {
            var existingImage = await _unitOfWork.ProductImages.GetByIdAsync(image.ImageId);
            if (existingImage == null)
                return null;

            existingImage.ImageUrl = image.ImageUrl;
            existingImage.AltText = image.AltText;
            existingImage.DisplayOrder = image.DisplayOrder;
            existingImage.IsPrimary = image.IsPrimary;

            _unitOfWork.ProductImages.Update(existingImage);
            await _unitOfWork.SaveChangesAsync();
            return existingImage;
        }

        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            var image = await _unitOfWork.ProductImages.GetByIdAsync(imageId);
            if (image == null)
                return false;

            _unitOfWork.ProductImages.Delete(image);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}