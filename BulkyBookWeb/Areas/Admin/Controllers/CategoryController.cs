using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Utility;
using BulkyBookWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IAuditLogService _auditLogService;
        public CategoryController(ICategoryService categoryService, IAuditLogService auditLogService)
        {
            _categoryService = categoryService;
            _auditLogService = auditLogService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePOST(Category category)
        {
            if (!String.IsNullOrEmpty(category.Name) && !await _categoryService.IsCategoryNameUniqueAsync(category.Name))
            {
                ModelState.AddModelError("", "Category name already exists");
            }

            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(category);
                await _auditLogService.LogAsync(User, SD.AuditCreated, SD.EntityCategory,
                      category.Id.ToString(), null, category.Name);
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Update(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Update")]
        public async Task<IActionResult> UpdatePOST(Category category)
        {
            if (!String.IsNullOrEmpty(category.Name) && !await _categoryService.IsCategoryNameUniqueAsync(category.Name,category.Id))
            {
                ModelState.AddModelError("", "Category name already exists");
            }

            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(category);
                await _auditLogService.LogAsync(User, SD.AuditUpdated, SD.EntityCategory,
    category.Id.ToString(), null, category.Name);
                TempData["success"] = "Category updated successfully";

                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var name = category.Name;
            await _categoryService.DeleteCategoryAsync(id);

            await _auditLogService.LogAsync(User, SD.AuditDeleted, SD.EntityCategory,
                 id.ToString(), name, null);

            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
