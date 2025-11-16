using knowledgeBase.Services;
using knowledgeBase.View_Models;

namespace knowledgeBase.Controllers;

public class CategoryController : BaseController
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task GetAllCategories(HttpContext context, Dictionary<string, string> parameters)
    {
        var categories = await _categoryService.GetAllCategories();
        await SendJsonAsync(context.Response, categories);
    }

    public async Task GetAllCategoryDTO(HttpContext context, Dictionary<string, string> parameters)
    {
        var categories = await _categoryService.GetAllCategories();
        var categoryDTOs = DTOMaker.MapCategories(categories);
        await SendJsonAsync(context.Response, categoryDTOs);
    }
}