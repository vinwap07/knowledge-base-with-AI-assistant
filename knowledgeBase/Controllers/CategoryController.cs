using knowledgeBase.Services;
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
}