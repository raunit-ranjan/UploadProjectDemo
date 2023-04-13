using Microsoft.EntityFrameworkCore;
using UploadProjectDemo.Data;
using UploadProjectDemo.Models.Domain;

namespace UploadProjectDemo.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext dbContext;
        public ProductRepository(ProductDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await dbContext.Products.ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await dbContext.Products.FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
