using System.Net;

namespace knowledgeBase.Middleware;

public interface IMiddleware
{
    Task InvokeAsync(HttpContext context, Func<Task> next);
}