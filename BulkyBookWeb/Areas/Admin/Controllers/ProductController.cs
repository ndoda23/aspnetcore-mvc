using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using BulkyBookWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICategoryService _categoryService;
        private readonly IAuditLogService _auditLogService;
        public ProductController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment, IAuditLogService auditLogService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
            _auditLogService = auditLogService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ProductVM productVM = new()
            {
                CategoryList = categories
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = new Product()
            };

            if(id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = await _productService.GetProductByIdAsync(id.Value);
                return View(productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Upsert")]
        public async Task<IActionResult> UpsertPOST(ProductVM productVM,IFormFile? file)
        {
           

            if (ModelState.IsValid)
            {
                string wwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    string productPath = Path.Combine("images","products");
                    string finalPath = Path.Combine(wwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);

                    // save the new image
                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = Path.Combine(@"\",productPath,fileName).Replace("\\","/");
                }

                bool isNew = productVM.Product.Id == 0;                        

                if (isNew)
                {
                    await _productService.CreateProductAsync(productVM.Product);
                    await _auditLogService.LogAsync(User, SD.AuditCreated, SD.EntityProduct,  
                        productVM.Product.Id.ToString(), null, productVM.Product.Title);
                }
                else
                {
                    await _productService.UpdateProductAsync(productVM.Product);
                    await _auditLogService.LogAsync(User, SD.AuditUpdated, SD.EntityProduct,   
                        productVM.Product.Id.ToString(), null, productVM.Product.Title);
                }


                TempData["success"] = isNew ? "Product created successfully"                  
                                           : "Product updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                productVM.CategoryList = categories
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                return View(productVM);
            }
        }








        #region API CALLS

        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync(true);
            return Json(new { data = products });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            var productToBeDeleted = await _productService.GetProductByIdAsync(id.Value);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var deletedTitle = productToBeDeleted.Title;

            if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\','/'));


                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _productService.DeleteProductAsync(id.Value);
            await _auditLogService.LogAsync(User, SD.AuditDeleted, SD.EntityProduct,     
               id.Value.ToString(), deletedTitle, null);
            return Json(new { success = true, message = "Delete Successful"});
        }

        #endregion
    }
}
