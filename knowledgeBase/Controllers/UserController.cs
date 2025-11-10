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
    
    public override async Task<string> HandleRequest(HttpListenerContext context)
    {
        switch (context.Request.HttpMethod)
        {
            case "GET":
	            return await GetUserProfile(context);
                break;
            case "POST":
	            switch (context.Request.Url.LocalPath)
	            {
		            case "/user/login":
			            return await LoginUser(context);
			            break;
		            case "/user/register":
			            return await RegisterNewUser(context);
			            break;
		            case "/user/logout":
			            return await LogoutUser(context);
			            break;
		            case "user/delete":
			            return await DeleteUserProfile(context);
			            break;
		            case "user/update":
			            return await UpdateUserProfile(context);
			            break;
		            case "/user/toFavorite":
			            return await AddArticleToFavorite(context);
			            break;
		            case "/user/toUnFavorite":
			            return await RemoveArticleFromFavorite(context);
			            break;
		            case "user/favorite":
			            return await GetFavoriteArticles(context);
		            default:
			            throw new FormatException("Unknown HTTP method");
	            }
	            break;
            case "DELETE":
	            return await DeleteUserProfile(context);
                break;
            default:
	            throw new FormatException("Unknown HTTP method");
        }
    }

    private async Task<string> UpdateUserProfile(HttpListenerContext context)
    {
	    var body = await ReadRequestBodyAsync(context.Request);

	    if (string.IsNullOrWhiteSpace(body))
	    {
		    throw new ArgumentException("Request body is empty");
	    }
	    
	    var cookie = context.Request.Cookies["SessoinId"];
	    if (cookie == null)
	    {
		    return "False";
	    }
        
	    try
	    {
		    var options = new JsonSerializerOptions
		    {
			    PropertyNameCaseInsensitive = true
		    };

		    var user = JsonSerializer.Deserialize<User>(body, options);
		    if (user == null)
		    {
			    throw new ArgumentException("Invalid request body");
		    }
		    
		    return await _userService.UpdateUser(user);
	    }
	    catch (JsonException ex)
	    {
		    throw new ArgumentException($"Invalid JSON format: {ex.Message}");
	    }
    }

    private async Task<string> GetFavoriteArticles(HttpListenerContext context)
    {
	    var cookie = context.Request.Cookies["SessoinId"];
	    if (cookie == null)
	    {
		    return "False";
	    }
	    
	    var articles = await _userService.GetAllArticlesBySessionId(cookie.Value);
	    return articles.Count > 0 ? JsonSerializer.Serialize(articles) : "False";
    }
    
    private async Task<string> AddArticleToFavorite(HttpListenerContext context)
    {
	    var query = context.Request.QueryString;
	    var articleId = query["articleId"];
	    var cookie = context.Request.Cookies["SessionId"];
	    var sessionId = cookie?.Value;
	    if (articleId == null || sessionId == null)
	    {
		    return "False";
	    }
	    
	    return await _userService.AddArticleToFavorite(sessionId, int.Parse(articleId)) ? "True" : "False";
    }

    private async Task<string> RemoveArticleFromFavorite(HttpListenerContext context)
    {
	    var query = context.Request.QueryString;
	    var articleId = query["articleId"];
	    var cookie = context.Request.Cookies["SessionId"];
	    var sessionId = cookie?.Value;
	    if (articleId == null || sessionId == null)
	    {
		    return "False";
	    }
	    
	    return await _userService.RemoveArticleFromFavorite(sessionId, int.Parse(articleId)) ? "True" : "False";
    }

    private async Task<string> LogoutUser(HttpListenerContext context)
    {
	    var cookie = context.Request.Cookies["SessionId"];
	    if (cookie == null)
	    {
		    return "False";
	    }
	    
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
	    
	    context.Response.Cookies.Add(cookie);
	    context.Response.StatusCode = (int)HttpStatusCode.OK;
	    return "True";
    }

    private async Task<string> LoginUser(HttpListenerContext context)
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
		    var sessionId = await _userService.CreateSession(user.Email);
		    SetSessionCookie(context, sessionId);
			
		    return userProfile.Email != string.Empty ? JsonSerializer.Serialize(userProfile) : "Failed";
	    }
	    catch (JsonException ex)
	    {
		    throw new ArgumentException($"Invalid JSON format: {ex.Message}");
	    }
    }

    private async Task<string> DeleteUserProfile(HttpListenerContext context)
    {
	    var cookie = context.Request.Cookies["SessionId"];
	    var isDeleted = cookie != null && await _userService.DeleteUserProfile(cookie.Value);
	    return isDeleted ? "True" : "False";
    }

    private async Task<string> GetUserProfile(HttpListenerContext context)
    {
	    var cookie = context.Request.Cookies["SessionId"];
	    var sessionId = cookie?.Value;
	    var userProfile = cookie != null ? await _userService.GetUserProfileBySessionId(cookie.Value) : null;
	    return userProfile != null ? JsonSerializer.Serialize(userProfile) : "False";
    }

    private async Task<string> RegisterNewUser(HttpListenerContext context)
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
		    if (user == null)
		    {
			    throw new ArgumentException("Invalid request body");
		    }
		    
		    var sessionId = await _userService.RegisterNewUser(user) ? await _userService.CreateSession(user.Email) : null;
		    if (sessionId != null)
		    {
			    SetSessionCookie(context, sessionId);
			    var userProfile = await _userService.GetUserProfileBySessionId(sessionId);
			    return JsonSerializer.Serialize(userProfile);
		    }

		    return "False";
	    }
	    catch (JsonException ex)
	    {
		    throw new ArgumentException($"Invalid JSON format: {ex.Message}");
	    }
    }
    
    private void SetSessionCookie(HttpListenerContext context, string sessionId)
    {
	    var expires = DateTime.Now.AddMinutes(60).ToString("R");
	    var cookieValue = $"SessionID={sessionId}; Expires={expires}; Path=/; HttpOnly; SameSite=Strict";
    
	    context.Response.Headers.Add("Set-Cookie", cookieValue);
    }
}