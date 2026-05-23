using KX_Project.Models;
using KX_Project.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _hostingEnvironment;
    public ProductController(IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IWebHostEnvironment hostingEnvironment)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _hostingEnvironment = hostingEnvironment;
    }

    public IActionResult Add()
    {
        var categories = _categoryRepository.GetAllCategories();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View();
    }
    [HttpPost]
    public IActionResult Add(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
                product.ImageUrl = "/images/" + uniqueFileName;
            }
            _productRepository.Add(product);
            return RedirectToAction("Index"); // Chuyển hướng tới trang danh sách sản phẩm
        }
        var categories = _categoryRepository.GetAllCategories();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(product);
    }
    // Các actions khác như Display, Update, Delete
    // Display a list of products
    public IActionResult Index()
    {
        var products = _productRepository.GetAll();
        return View(products);
    }
    // Display a single product
    public IActionResult Display(int id)
    {
        var product = _productRepository.GetById(id);
        if (product == null)
        {
            return NotFound();
        }
        var relatedProducts = _productRepository.GetAll()
            .Where(p => p.Id != id && p.CategoryId == product.CategoryId)
            .Take(4)
            .ToList();
        
        // If not enough products in same category, get any
        if (relatedProducts.Count < 4)
        {
            var moreProducts = _productRepository.GetAll()
                .Where(p => p.Id != id && !relatedProducts.Any(rp => rp.Id == p.Id))
                .Take(4 - relatedProducts.Count)
                .ToList();
            relatedProducts.AddRange(moreProducts);
        }
        
        ViewBag.RelatedProducts = relatedProducts;
        return View(product);
    }
    // Show the product update form
    public IActionResult Update(int id)
    {
        var product = _productRepository.GetById(id);
        if (product == null)
        {
            return NotFound();
        }
        var categories = _categoryRepository.GetAllCategories();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(product);
    }
    // Process the product update
    [HttpPost]
    public IActionResult Update(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
                product.ImageUrl = "/images/" + uniqueFileName;
            }
            else
            {
                // Giữ nguyên ảnh cũ nếu không upload ảnh mới
                var existingProduct = _productRepository.GetById(product.Id);
                if (existingProduct != null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
            }
            _productRepository.Update(product);
            return RedirectToAction("Index");
        }
        var categories = _categoryRepository.GetAllCategories();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(product);
    }
    // Show the product delete confirmation
    public IActionResult Delete(int id)
    {
        var product = _productRepository.GetById(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
    // Process the product deletion
    [HttpPost, ActionName("DeleteConfirmed")]
    public IActionResult DeleteConfirmed(int id)
    {
        _productRepository.Delete(id);
        return RedirectToAction("Index");
    }
}