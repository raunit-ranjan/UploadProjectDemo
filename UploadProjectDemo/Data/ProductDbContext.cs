using Microsoft.EntityFrameworkCore;
using UploadProjectDemo.Models.Domain;

namespace UploadProjectDemo.Data
{
    public class ProductDbContext: DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
           : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

    }
}
