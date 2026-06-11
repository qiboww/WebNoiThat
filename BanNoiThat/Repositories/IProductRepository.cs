using BanNoiThat.Models;

namespace BanNoiThat.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int? categoryId);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product?> GetProductWithCategoryAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
    }
}
