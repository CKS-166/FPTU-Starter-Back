using FPTU_Starter.Application.Services;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.CategoryDTO;
using Microsoft.AspNetCore.Mvc;

namespace FPTU_Starter.API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryManagementController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryManagementController(ICategoryService categoryService) { 
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCates()
        {
            var result = _categoryService.ViewAllCates();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCate(CategoryAddRequest request)
        {
            var result = _categoryService.CreateCate(request);
            return Ok(result);
        }
    }
}
