using System.Net;
using System.Security.Authentication;
using System.Text;
using knowledgeBase.Controllers;
namespace knowledgeBase.Middleware;

public class RoutingMiddleware : IMiddleware
{
    private readonly RouteTable _routeTable;

    public RoutingMiddleware(RouteTable routes)
    {
        _routeTable = routes;
    }

    public async Task InvokeAsync(HttpContext context, Func<Task> next)
    {
        var request = context.Request;
        var response = context.Response;
        string method = request.HttpMethod;
        string path = request.Url.LocalPath;
                
        if (path.Contains(".well-known/appspecific/com.chrome.devtools.json"))
        {
            await HandleChromeDevToolsRequest(context);
            return;
        }
            
        Console.WriteLine($"'{method}': {path}");
        var route = _routeTable.FindRoute(method, path);

        if (route == null)
        {
            await next();
        }
        else
        {
            if (!route.CanUserByUnknown && context.Role == "unknown")
            {
                response.StatusCode = 401;
                throw new UnauthorizedAccessException();
            }
            else
            {
                var parameters = route.ExtractParameters(path);
                await route.Handler(context, parameters);
            }
        }
    }
    
    private async Task HandleChromeDevToolsRequest(HttpContext context)
    {
        var response = context.Response;
            
        var devToolsResponse = new
        {
            supported = false,
            message = "Chrome DevTools Protocol not supported by this server"
        };
            
        string json = System.Text.Json.JsonSerializer.Serialize(devToolsResponse);
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
            
        response.StatusCode = 200;
        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;
            
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
            
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Chrome DevTools request handled");
    }
}

