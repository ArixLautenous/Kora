using KX_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KX_Project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductsApi
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Images = p.Images != null ? p.Images.Select(i => i.Url) : new List<string>()
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/ProductsApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Images = p.Images != null ? p.Images.Select(i => i.Url) : new List<string>()
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            }

            return Ok(product);
        }
    }
}
