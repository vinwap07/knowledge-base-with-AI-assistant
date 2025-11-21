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

    public async Task CheckLike(HttpContext context, Dictionary<string, string> parameters)
    {
        var articleId = parameters["articleId"];
        var sessionId = CookieHelper.GetCookieValue(context.Request, "sessionID");
        var isLiked = await _articleService.CheckLike(int.Parse(articleId), sessionId);
        await WriteResponseAsync(context.Response, isLiked.ToString());
    }
    public async Task CreateArticle(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "sessionID");
        var article = await FromJsonBodyAsync<Article>(context.Request);
        var articleId = await _articleService.CreateArticle(sessionId, article, context.Role);
        await WriteResponseAsync(context.Response, articleId.ToString());
    }

    public async Task GetArticlePage(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        var articleId = parameters["articleId"];
        var isNum = Int32.TryParse(articleId, out int num);
        if (articleId == null || !isNum)
        {
            throw new FileNotFoundException();
        }
        
        var article = await _articleService.GetArticleById(num, sessionId);
        var html = PageCreator.GetArticlePage(article);
        context.Response.ContentType = "text/html; charset=utf-8";
        await WriteResponseAsync(context.Response, html);
    }

    public async Task GetMyArticlePreview(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        var user = await _userService.GetUserBySessionId(sessionId);
        var articles = await _articleService.GetArticlesByAuthor(user.Email, sessionId);
        var articlePreviews = DTOMaker.MapArticles(articles);
        await SendJsonAsync(context.Response, articlePreviews);
    }

    public async Task GetPopularArticlesPreview(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        var articles = await _articleService.GetPopularArticles(int.Parse(parameters["count"]), sessionId);
        var articlePreviews = DTOMaker.MapArticles(articles);
        await SendJsonAsync(context.Response, articlePreviews);
    }
    
    public async Task GetFavoriteArticlesPreview(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        var articles = await _articleService.GetFavouriteArticles(sessionId);
        var articlePreviews = DTOMaker.MapArticles(articles);
        await SendJsonAsync(context.Response, articlePreviews);
    }
    
    public async Task LikeArticle(HttpContext context, Dictionary<string, string> parameters)
    {
        var articleId = parameters["articleId"];
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        if (articleId == null)
        {
            throw new FileNotFoundException();
        }

        var IsAdded = await _articleService.LikeArticle(sessionId, int.Parse(articleId));
        await SendTextAsync(context.Response, IsAdded ? "True" : "False");
    }
    
    public async Task RemoveArticleFromLiked(HttpContext context, Dictionary<string, string> parameters)
    {
        var articleId = parameters["articleId"];
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        if (articleId == null)
        {
            throw new FileNotFoundException();
        }
	    
        var isRemoved = await _articleService.RemoveArticleFromLiked(sessionId, int.Parse(articleId));
        await SendTextAsync(context.Response, isRemoved ? "True" : "False");
    }

    public async Task GetAllArticleDTO(HttpContext context, Dictionary<string, string> parameters)
    {
        var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
        var articles = await _articleService.GetAllArticles(sessionId);
        var articleDTOs = DTOMaker.MapArticles(articles);
        await SendJsonAsync(context.Response, articleDTOs);
    }
    
    // TODO: SearchArticlesByCategory
}