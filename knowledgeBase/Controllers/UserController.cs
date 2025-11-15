using System.Net;
using System.Text;
using System.Text.Json;
using knowledgeBase.Entities;
using knowledgeBase.Services;
using knowledgeBase.View_Models;

namespace knowledgeBase.Controllers;

public class UserController : BaseController
{
    private UserService _userService;
    private ArticleService _articleService;

    public UserController(UserService userService, ArticleService articleService)
    {
	    _userService = userService;
	    _articleService = articleService;
    }
    
    public async Task UpdateUserProfile(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var body = await ReadRequestBodyAsync(context.Request);

	    if (string.IsNullOrWhiteSpace(body))
	    {
		    throw new ArgumentException("Request body is empty");
	    }
	    
	    var cookie = context.Request.Cookies["SessoinId"];
	    if (cookie == null)
	    {
		    await SendTextAsync(context.Response, "False");
	    }
        
	    try
	    {
		    var user = await FromJsonBodyAsync<User>(context.Request);
		    if (user == null)
		    {
			    throw new ArgumentException("Invalid request body");
		    }
		    
		    var userProfile = await _userService.UpdateUser(user);
		    await SendJsonAsync(context.Response, ToJson(userProfile));
	    }
	    catch (JsonException ex)
	    {
		    throw new ArgumentException($"Invalid JSON format: {ex.Message}");
	    }
    }

    public async Task GetFavoriteArticlesPreview(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var cookie = context.Request.Cookies["SessionID"];
	    if (cookie == null)
	    {
		    await SendTextAsync(context.Response, "False");
		    return;
	    }
	    
	    var articles = await _userService.GetAllArticlesBySessionId(cookie.Value);
	    var articlePreviews = new List<ArticlePreviewModel>();
	    foreach (var article in articles)
	    {
		    var likesCount = await _articleService.GetArticleLikesCount(article.Id);
		    articlePreviews.Add(new ArticlePreviewModel()
		    {
			    Id = article.Id,
			    Author = article.Author,
			    Title = article.Title,
			    Summary = article.Summary,
			    PublishDate = article.PublishDate,
			    ReadingTime = article.ReadingTime,
			    LikeCount = (int)likesCount
		    });
	    }
	    
	    if (articles.Count > 0)
	    {
		    await SendJsonAsync(context.Response, articlePreviews);
	    }
	    else
	    {
		    await SendTextAsync(context.Response, "False");
	    }
    }
    
    public async Task AddArticleToFavorite(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var query = context.Request.QueryString;
	    var articleId = query["articleId"];
	    var cookie = context.Request.Cookies["SessionID"];
	    var sessionId = cookie?.Value;
	    if (articleId == null || sessionId == null)
	    {
		    await SendTextAsync(context.Response, "False");
	    }

	    var IsAdded = await _userService.AddArticleToFavorite(sessionId, int.Parse(articleId));
	    await SendTextAsync(context.Response, IsAdded ? "True" : "False");
    }

    public async Task RemoveArticleFromFavorite(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var query = context.Request.QueryString;
	    var articleId = query["articleId"];
	    var cookie = context.Request.Cookies["SessionId"];
	    var sessionId = cookie?.Value;
	    if (articleId == null || sessionId == null)
	    {
		    await SendTextAsync(context.Response, "False");
	    }
	    
	    var isRemoved = await _userService.RemoveArticleFromFavorite(sessionId, int.Parse(articleId));
	    await SendTextAsync(context.Response, isRemoved ? "True" : "False");
    }

    public async Task LogoutUser(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var cookie = context.Request.Cookies["SessionId"];
	    var nameCookie = context.Request.Cookies["Name"];
	    if (cookie != null)
	    {
		    var isLogout = await _userService.LogoutUser(cookie.Value);

		    if (!isLogout)
		    {
			    throw new FormatException("Logout failed");
		    }
	    
		    cookie = new Cookie("SessionID", "")
		    {
			    Expires = DateTime.Now.AddDays(-1),
			    Path = "/"
		    };
		    nameCookie = new Cookie("Name", "")
		    {
			    Expires = DateTime.Now.AddDays(-1),
			    Path = "/"
		    };
		    context.Response.Cookies.Add(cookie);
		    context.Response.Cookies.Add(nameCookie);
	    }
	    
	    context.Response.StatusCode = (int)HttpStatusCode.OK;
	    await SendJsonAsync(context.Response, "True");
	    
    }

    public async Task LoginUser(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var body = await ReadRequestBodyAsync(context.Request);

	    if (string.IsNullOrWhiteSpace(body))
	    {
		    throw new ArgumentException("Request body is empty");
	    }
        
	    try
	    {
		    var options = new JsonSerializerOptions
		    {
			    PropertyNameCaseInsensitive = true
		    };

		    var user = JsonSerializer.Deserialize<User>(body, options);
		    var userProfile = await _userService.Authenticate(user.Email, user.Password);

		    if (userProfile.Email != null)
		    {
			    var sessionId = await _userService.CreateSession(user.Email);
			    SetCookie(context, sessionId, "SessionID");
			    SetCookie(context, userProfile.Name, "Name");
			    await SendJsonAsync(context.Response, ToJson(userProfile));
		    }
		    else
		    {
			    await SendTextAsync(context.Response, "Wrong email or password");
		    }
	    }
	    catch (JsonException ex)
	    {
		    throw new ArgumentException($"Invalid JSON format: {ex.Message}");
	    }
    }

    public async Task DeleteUserProfile(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var cookie = context.Request.Cookies["SessionID"];
	    var isDeleted = cookie != null && await _userService.DeleteUserProfile(cookie.Value);
	    await SendTextAsync(context.Response, isDeleted ? "True" : "False");
    }

    public async Task GetUserProfile(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var sessionId = context.Request.Cookies["SessionID"].Value;
	    var userProfile = await _userService.GetUserProfileBySessionId(sessionId);
	    await SendJsonAsync(context.Response, ToJson(userProfile));
    }

    public async Task RegisterNewUser(HttpListenerContext context, Dictionary<string, string> parameters)
    {
	    var user = await FromJsonBodyAsync<User>(context.Request);
        if (user == null)
        {
            throw new ArgumentException("Request body is empty");
        }

        user.RoleId = 1;
        try
	    {
		    var result = await _userService.RegisterNewUser(user);
		    if (result == "Email already exists")
		    {
			    await SendTextAsync(context.Response, "Данный адрес электронной почты уже зарегистрирован");
		    }
		    else
		    {
			    var sessionId = await _userService.CreateSession(user.Email);
			    if (sessionId != null)
			    {
				    SetCookie(context, sessionId, "SessionID");
				    SetCookie(context, user.Name, "Name");
				    await SendJsonAsync(context.Response, string.Empty);
			    }
			    else
			    {
				    await SendTextAsync(context.Response, "Что-то пошло не так, попробуйте снова");
			    }
		    }
	    }
	    catch (Exception ex)
	    {
		    throw new ArgumentException(ex.Message);
	    }
    }

    private void SetCookie(HttpListenerContext context, string value, string cookieName)
    {
	    var expires = DateTime.Now.AddMinutes(60).ToString("R");
	    var cookieValue = $"{cookieName}={value}; Expires={expires}; Path=/; SameSite=Strict";
	    context.Response.Headers.Add("Set-Cookie", cookieValue);
    }
}