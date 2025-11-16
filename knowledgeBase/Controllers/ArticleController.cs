using System.Net;
using System.Security.Authentication;
using knowledgeBase.Entities;
using knowledgeBase.Services;
using knowledgeBase.View_Models;

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
    public async Task CreateArticle(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "sessionID");
        var article = await FromJsonBodyAsync<Article>(context.Request);
        var user = await _userService.GetUserBySessionId(sessionId);
        var articleId = await _articleService.CreateArticle(article, user, context.Role);
        await WriteResponseAsync(context.Response, articleId.ToString());
    }

    public async Task GetArticlePage(HttpContext context, Dictionary<string, string> parameters)
    {
        CookieHelper.GetCookieValue(context.Request, "SessionID");
        var articleId = parameters["articleId"];
        var isNum = Int32.TryParse(articleId, out int num);
        if (articleId == null || !isNum)
        {
            throw new FileNotFoundException();
        }
        
        var article = await _articleService.GetArticleById(num);
        var html = PageCreator.GetArticlePage(article);
        context.Response.ContentType = "text/html; charset=utf-8";
        await WriteResponseAsync(context.Response, html);
    }

    public async Task GetMyArticlePreview(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        var user = await _userService.GetUserBySessionId(sessionId);
        var articles = await _articleService.GetArticlesByAuthor(user.Email);
        var articlePreviews = DTOMaker.MapArticles(articles);
        await SendJsonAsync(context.Response, articlePreviews);
    }

    public async Task GetPopularArticlesPreview(HttpContext context, Dictionary<string, string> parameters)
    {
        var articles = await _articleService.GetPopularArticles(int.Parse(parameters["count"]));
        var articlePreviews = DTOMaker.MapArticles(articles);
        await SendJsonAsync(context.Response, articlePreviews);
    }
    
    public async Task GetFavoriteArticlesPreview(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        var articles = await _userService.GetFavouriteArticles(sessionId);
        var articlePreviews = DTOMaker.MapArticles(articles);
        await SendJsonAsync(context.Response, articlePreviews);
    }
    
    public async Task AddArticleToFavorite(HttpContext context, Dictionary<string, string> parameters)
    {
        var query = context.Request.QueryString;
        var articleId = query["articleId"];
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        if (articleId == null)
        {
            throw new FileNotFoundException();
        }

        var IsAdded = await _articleService.AddArticleToFavorite(sessionId, int.Parse(articleId));
        await SendTextAsync(context.Response, IsAdded ? "True" : "False");
    }
    
    public async Task RemoveArticleFromFavorite(HttpContext context, Dictionary<string, string> parameters)
    {
        var query = context.Request.QueryString;
        var articleId = query["articleId"];
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        if (articleId == null)
        {
            throw new FileNotFoundException();
        }
	    
        var isRemoved = await _articleService.RemoveArticleFromFavorite(sessionId, int.Parse(articleId));
        await SendTextAsync(context.Response, isRemoved ? "True" : "False");
    }
    
    // TODO: SearchArticlesByCategory
}