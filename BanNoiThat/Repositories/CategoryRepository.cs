using BanNoiThat.Data;
using BanNoiThat.Models;
using Microsoft.EntityFrameworkCore;

namespace BanNoiThat.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category?> GetCategoryWithProductsAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CategoryId == id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(e => e.CategoryId == id);
        }
    }
}
