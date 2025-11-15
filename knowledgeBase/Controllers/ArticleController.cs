using System.Net;
using System.Security.Authentication;
using knowledgeBase.Entities;
using knowledgeBase.Services;

namespace knowledgeBase.Controllers;

public class ArticleController : BaseController
{
    private ArticleService _articleService;
    private UserService _userService;

    public ArticleController(ArticleService articleService, UserService userService)
    {
        _articleService = articleService;
        _userService = userService;
    }
    public async Task CreateArticle(HttpListenerContext context, Dictionary<string, string> parameters)
    {
        var cookie  = context.Request.Cookies["SessionID"];
        if (cookie == null)
        {
            context.Response.StatusCode = 401;
            await WriteResponseAsync(context.Response, "unknown");
        }
        else
        {
            var article = await FromJsonBodyAsync<Article>(context.Request);
            var user = await _userService.GetUserProfileBySessionId(cookie.Value);
            article.PublishDate = DateOnly.FromDateTime(DateTime.Now);
            article.Author = user.Email;
            var role = await _userService.GetRoleBySessionId(cookie.Value);
            if (role == null || role == "user")
            {
                context.Response.StatusCode = 401;
                await WriteResponseAsync(context.Response, "unknown");
                return;
            }
            
            var articleId = await _articleService.CreateArticle(article, role);
            if (articleId == null)
            {
                await SendTextAsync(context.Response, "False", 401);
                return;
            }
            
            await WriteResponseAsync(context.Response, articleId.ToString());
        }
    }

    public async Task GetArticlePage(HttpListenerContext context, Dictionary<string, string> parameters)
    {
        var cookie  = context.Request.Cookies["SessionID"];
        if (cookie == null)
        {
            context.Response.StatusCode = 401;
            throw new UnauthorizedAccessException();
        }

        var articleId = parameters["articleId"];
        var isNum = Int32.TryParse(articleId, out int num);
        if (articleId == null || !isNum)
        {
            throw new FileNotFoundException();
            return;
        }
        
        var article = await _articleService.GetArticleById(num);
        var html = PageCreator.GetArticlePage(article);
        context.Response.ContentType = "text/html; charset=utf-8";
        await WriteResponseAsync(context.Response, html);
    }

    public async Task GetArticlePreviewByAuthor(HttpListenerContext context, Dictionary<string, string> parameters)
    {
        
    }
}