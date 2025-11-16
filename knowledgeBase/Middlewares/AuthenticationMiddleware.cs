using System.Net;
using knowledgeBase.Repositories;
using knowledgeBase.Services;

namespace knowledgeBase.Middleware;

public class AuthenticationMiddleware : IMiddleware
{
    private readonly UserService _userService;
    private readonly SessionRepository _sessionRepository;

    public AuthenticationMiddleware(UserService userService, SessionRepository sessionRepository)
    {
        _userService = userService;
        _sessionRepository = sessionRepository;
    }
    public async Task InvokeAsync(HttpContext context, Func<Task> next)
    {
        var cookie = context.Request.Cookies["SessionID"];
            
        if (cookie != null)
        {
            var sessionId = cookie.Value;
            Console.WriteLine(sessionId);
            var session = await _sessionRepository.GetById(sessionId);
            if (session != null)
            {
                if (session.EndTime <= DateTime.Now)
                {
                    await _sessionRepository.Delete(sessionId);
                }
            }
            
            context.Role = await _userService.GetRoleBySessionId(sessionId);
            if (context.Role == "unknown")
            {
                CookieHelper.DeleteCookie(context.Response, "SessionID");
                CookieHelper.DeleteCookie(context.Response, "Name");
            }
        }
        await next();
    }
}