using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenuShop.Data;
using MenuShop.Models;
using System.Security.Claims;

namespace MenuShop.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private bool IsAdmin()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated == true)
            {
                return false;
            }

            // Check for admin claim
            var adminClaim = User.FindFirst("IsAdmin");
            if (adminClaim != null && adminClaim.Value == "True")
            {
                return true;
            }

            // Alternative check using ClaimTypes
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null && roleClaim.Value == "Admin")
            {
                return true;
            }

            return false;
        }

        // Debug action to check user claims
        public IActionResult DebugUser()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Content("User not authenticated");
            }

            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            return Content("User Claims:\n" + string.Join("\n", claims));
        }

        // Categories Management
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("Id")?.Value;
            var categories = _context.Categories.AsQueryable();
            return View(await categories.ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("Id")?.Value;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category, IFormFile? imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    category.ImageUrl = await SaveFile(imageFile, "categories");
                }

                category.CreatedAt = DateTime.Now;
                category.UpdatedAt = DateTime.Now;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, Category category, IFormFile? imageFile, string? RemoveImage)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                // Xử lý xóa ảnh nếu RemoveImage == true
                if (RemoveImage == "true" && !string.IsNullOrEmpty(existingCategory.ImageUrl))
                {
                    // Xóa file vật lý nếu tồn tại
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCategory.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    existingCategory.ImageUrl = null;
                }
                else if (imageFile != null && imageFile.Length > 0)
                {
                    existingCategory.ImageUrl = await SaveFile(imageFile, "categories");
                }
                // Nếu không upload ảnh mới và không xóa thì giữ nguyên ảnh cũ

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }

        // Products Management
        public async Task<IActionResult> Products()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("Id")?.Value;
            var products = _context.Products.Include(p => p.Category).Include(p => p.Images).AsQueryable();
            return View(await products.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? imageFile0, IFormFile? imageFile1, IFormFile? imageFile2, IFormFile? imageFile3, IFormFile? imageFile4, IFormFile? videoFile)
        {
            Console.WriteLine("=== CREATE PRODUCT CONTROLLER START ===");
            Console.WriteLine($"IsAdmin: {IsAdmin()}");
            
            if (!IsAdmin()) 
            {
                Console.WriteLine("User is not admin, redirecting to AccessDenied");
                return RedirectToAction("AccessDenied", "Account");
            }
            
            // Collect all image files
            var imageFiles = new List<IFormFile>();
            if (imageFile0 != null && imageFile0.Length > 0) imageFiles.Add(imageFile0);
            if (imageFile1 != null && imageFile1.Length > 0) imageFiles.Add(imageFile1);
            if (imageFile2 != null && imageFile2.Length > 0) imageFiles.Add(imageFile2);
            if (imageFile3 != null && imageFile3.Length > 0) imageFiles.Add(imageFile3);
            if (imageFile4 != null && imageFile4.Length > 0) imageFiles.Add(imageFile4);
            
            // Debug logging
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Product Name: {product.Name}");
            Console.WriteLine($"Product CategoryId: {product.CategoryId}");
            Console.WriteLine($"Product Price: {product.Price}");
            Console.WriteLine($"Product Stock: {product.Stock}");
            Console.WriteLine($"Product IsActive: {product.IsActive}");
            Console.WriteLine($"Product DiscountPercent: {product.DiscountPercent}");
            Console.WriteLine($"Product DiscountStartDate: {product.DiscountStartDate}");
            Console.WriteLine($"Product DiscountEndDate: {product.DiscountEndDate}");
            Console.WriteLine($"Number of image files: {imageFiles.Count}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== VALIDATION ERRORS ===");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                Console.WriteLine("=== END VALIDATION ERRORS ===");
            }
            
            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid, proceeding to save...");
                
                // Validate number of images (max 5)
                if (imageFiles.Count > 5)
                {
                    ModelState.AddModelError("", "Chỉ được upload tối đa 5 ảnh");
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    return View(product);
                }

                // Save video if provided
                if (videoFile != null && videoFile.Length > 0)
                {
                    product.VideoUrl = await SaveFile(videoFile, "products");
                    Console.WriteLine($"Video saved: {product.VideoUrl}");
                }

                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;

                Console.WriteLine("Adding product to context...");
                _context.Products.Add(product);
                
                // Save product first to get the ID
                await _context.SaveChangesAsync();
                Console.WriteLine($"Product saved with ID: {product.Id}");
                
                // Save images if provided
                if (imageFiles.Any())
                {
                    Console.WriteLine("Saving images...");
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var imageUrl = await SaveFile(imageFile, "products");
                            var productImage = new ProductImage
                            {
                                ImageUrl = imageUrl,
                                ProductId = product.Id
                            };
                            _context.ProductImages.Add(productImage);
                            Console.WriteLine($"Image saved: {imageUrl}");
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                
                Console.WriteLine("Product and images saved successfully, redirecting to Products list");
                return RedirectToAction(nameof(Products));
            }

            Console.WriteLine("ModelState is invalid, returning to view");
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? imageFile0, IFormFile? imageFile1, IFormFile? imageFile2, IFormFile? imageFile3, IFormFile? imageFile4, IFormFile? videoFile)
        {
            Console.WriteLine("=== EDIT PRODUCT CONTROLLER START ===");
            Console.WriteLine($"IsAdmin: {IsAdmin()}");
            Console.WriteLine($"Product ID: {id}");
            
            if (!IsAdmin()) 
            {
                Console.WriteLine("User is not admin, redirecting to AccessDenied");
                return RedirectToAction("AccessDenied", "Account");
            }
            
            if (id != product.Id)
            {
                Console.WriteLine($"ID mismatch: {id} != {product.Id}");
                return NotFound();
            }

            // Collect all image files
            var imageFiles = new List<IFormFile>();
            if (imageFile0 != null && imageFile0.Length > 0) imageFiles.Add(imageFile0);
            if (imageFile1 != null && imageFile1.Length > 0) imageFiles.Add(imageFile1);
            if (imageFile2 != null && imageFile2.Length > 0) imageFiles.Add(imageFile2);
            if (imageFile3 != null && imageFile3.Length > 0) imageFiles.Add(imageFile3);
            if (imageFile4 != null && imageFile4.Length > 0) imageFiles.Add(imageFile4);

            // Debug logging
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Product Name: {product.Name}");
            Console.WriteLine($"Product CategoryId: {product.CategoryId}");
            Console.WriteLine($"Product Price: {product.Price}");
            Console.WriteLine($"Product Stock: {product.Stock}");
            Console.WriteLine($"Product IsActive: {product.IsActive}");
            Console.WriteLine($"Product DiscountPercent: {product.DiscountPercent}");
            Console.WriteLine($"Product DiscountStartDate: {product.DiscountStartDate}");
            Console.WriteLine($"Product DiscountEndDate: {product.DiscountEndDate}");
            Console.WriteLine($"Number of image files: {imageFiles.Count}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== VALIDATION ERRORS ===");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                Console.WriteLine("=== END VALIDATION ERRORS ===");
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid, proceeding to update...");
                
                var existingProduct = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (existingProduct == null)
                {
                    Console.WriteLine("Existing product not found");
                    return NotFound();
                }

                Console.WriteLine("Found existing product, updating...");

                // Validate number of images (max 5 total)
                var currentImageCount = existingProduct.Images.Count;
                var newImageCount = imageFiles.Count;
                if (currentImageCount + newImageCount > 5)
                {
                    ModelState.AddModelError("", $"Sản phẩm hiện có {currentImageCount} ảnh. Chỉ được thêm tối đa {5 - currentImageCount} ảnh nữa.");
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    return View(existingProduct);
                }

                // Save video if provided
                if (videoFile != null && videoFile.Length > 0)
                {
                    product.VideoUrl = await SaveFile(videoFile, "products");
                    Console.WriteLine($"New video saved: {product.VideoUrl}");
                }
                else
                {
                    product.VideoUrl = existingProduct.VideoUrl;
                    Console.WriteLine($"Keeping existing video: {product.VideoUrl}");
                }

                // Update product properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.VideoUrl = product.VideoUrl;
                existingProduct.IsActive = product.IsActive;
                existingProduct.DiscountPercent = product.DiscountPercent;
                existingProduct.DiscountStartDate = product.DiscountStartDate;
                existingProduct.DiscountEndDate = product.DiscountEndDate;
                existingProduct.UpdatedAt = DateTime.Now;

                // Save new images if provided
                if (imageFiles.Any())
                {
                    Console.WriteLine("Saving new images...");
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var imageUrl = await SaveFile(imageFile, "products");
                            var productImage = new ProductImage
                            {
                                ImageUrl = imageUrl,
                                ProductId = existingProduct.Id
                            };
                            _context.ProductImages.Add(productImage);
                            Console.WriteLine($"New image saved: {imageUrl}");
                        }
                    }
                }

                Console.WriteLine("Saving changes to database...");
                await _context.SaveChangesAsync();
                
                Console.WriteLine("Product updated successfully, redirecting to Products list");
                return RedirectToAction(nameof(Products));
            }

            Console.WriteLine("ModelState is invalid, returning to view");
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/DeleteImage/{imageId}")]
        public async Task<IActionResult> DeleteImage(string imageId)
        {
            Console.WriteLine($"DeleteImage called with imageId={imageId}");
            var allIds = _context.ProductImages.Select(x => x.Id).ToList();
            Console.WriteLine("All ProductImage IDs: " + string.Join(", ", allIds));
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            
            var productImage = await _context.ProductImages.FindAsync(imageId);
            if (productImage != null)
            {
                // Delete physical file
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, productImage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                
                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();
                var afterIds = _context.ProductImages.Select(x => x.Id).ToList();
                Console.WriteLine("After delete, ProductImages IDs: " + string.Join(", ", afterIds));
                
                Console.WriteLine($"Deleted ProductImage with id={imageId}. Remaining IDs: " + string.Join(", ", afterIds));
                return Json(new { success = true });
            }
            Console.WriteLine($"ImageId {imageId} NOT FOUND in ProductImages");
            return Json(new { success = false, message = "Không tìm thấy ảnh" });
        }

        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + folder + "/" + uniqueFileName;
        }
    }
} 