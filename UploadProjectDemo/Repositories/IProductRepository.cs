using UploadProjectDemo.Models.Domain;

namespace UploadProjectDemo.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);

    }
}
