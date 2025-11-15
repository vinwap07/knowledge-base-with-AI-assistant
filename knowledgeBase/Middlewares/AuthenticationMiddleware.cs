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
    public async Task InvokeAsync(HttpContext myContext, Func<Task> next)
    {
        var context = myContext.Context;
        var cookie = context.Request.Cookies["SessionID"];
            
        if (cookie != null)
        {
            var sessionId = cookie.Value;
            Console.WriteLine(sessionId);
            var session = await _sessionRepository.GetById(sessionId);
            if (session.EndTime >= DateTime.Now)
            {
                await _sessionRepository.Delete(sessionId);
            }
            
            myContext.Role = await _userService.GetRoleBySessionId(sessionId);
        }
        await next();
    }
}