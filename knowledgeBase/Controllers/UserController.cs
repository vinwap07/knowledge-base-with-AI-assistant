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
    
    public async Task UpdateUserProfile(HttpContext context, Dictionary<string, string> parameters)
    {
	    var body = await ReadRequestBodyAsync(context.Request);

	    if (string.IsNullOrWhiteSpace(body))
	    {
		    throw new ArgumentException("Request body is empty");
	    }
	    
	    CookieHelper.GetCookieValue(context.Request, "SessionID");
	    
	    try
	    {
		    var user = await FromJsonBodyAsync<User>(context.Request);
		    if (user == null)
		    {
			    throw new ArgumentException("Invalid request body");
		    }
		    
		    await _userService.UpdateUser(user);
		    var userProfile = DTOMaker.MapUser(user, context.Role);
		    await SendJsonAsync(context.Response, ToJson(userProfile));
	    }
	    catch (JsonException ex)
	    {
		    throw new ArgumentException($"Invalid JSON format: {ex.Message}");
	    }
    }

    public async Task LogoutUser(HttpContext context, Dictionary<string, string> parameters)
    {
	    var sessionID = CookieHelper.GetCookieValue(context.Request, "SessionID");
	    var isLogout = await _userService.LogoutUser(sessionID);

	    if (!isLogout)
	    {
		    throw new FormatException("Logout failed");
	    }

	    CookieHelper.DeleteCookie(context.Response, "SessionID");
	    CookieHelper.DeleteCookie(context.Response, "Name");
	    
	    context.Response.StatusCode = (int)HttpStatusCode.OK;
	    await SendJsonAsync(context.Response, "True");
	    
    }

    public async Task LoginUser(HttpContext context, Dictionary<string, string> parameters)
    {
	    var body = await ReadRequestBodyAsync(context.Request);
	    try
	    {
		    var options = new JsonSerializerOptions
		    {
			    PropertyNameCaseInsensitive = true
		    };

		    var incomingUser = JsonSerializer.Deserialize<User>(body, options);
		    var user = await _userService.Authenticate(incomingUser);

		    if (user.Email != null)
		    {
			    var sessionId = await _userService.CreateSession(user);
			    CookieHelper.SetCookie(context.Response, sessionId, "SessionID");
			    CookieHelper.SetCookie(context.Response, user.Name, "Name");
			    var userProfile = DTOMaker.MapUser(user, context.Role);
			    await SendJsonAsync(context.Response, userProfile);
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

    public async Task DeleteUserProfile(HttpContext context, Dictionary<string, string> parameters)
    {
	    var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
	    await _userService.DeleteUserProfile(sessionId);
	    context.Response.StatusCode = (int)HttpStatusCode.OK;
    }

    public async Task GetUserProfile(HttpContext context, Dictionary<string, string> parameters)
    {
	    var sessionId = CookieHelper.GetCookieValue(context.Request, "SessionID");
	    var user = await _userService.GetUserBySessionId(sessionId);
	    var userProfile = DTOMaker.MapUser(user, context.Role);
	    await SendJsonAsync(context.Response, userProfile);
    }

    public async Task RegisterNewUser(HttpContext context, Dictionary<string, string> parameters)
    {
	    var user = await FromJsonBodyAsync<User>(context.Request);
        if (user == null)
        {
            throw new ArgumentException("Request body is empty");
        }
        
        try
	    {
		    var result = await _userService.RegisterNewUser(user);
		    if (result == "Email already exists")
		    {
			    await SendTextAsync(context.Response, "Данный адрес электронной почты уже зарегистрирован");
		    }
		    else
		    {
			    var sessionId = await _userService.CreateSession(user);
			    if (sessionId != null)
			    {
				    CookieHelper.SetCookie(context.Response, sessionId, "SessionID");
				    CookieHelper.SetCookie(context.Response, user.Name, "Name");
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
}