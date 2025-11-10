using System.Net;
using System.Text;
using knowledgeBase.Controllers;
using knowledgeBase.Entities;

namespace knowledgeBase.Middleware;

public class RoutingMiddleware : IMiddleware
{
    private UserController _userController;
    //private ArticleController _articleController;
    public RoutingMiddleware(UserController userController)
    {
        _userController = userController;
    }
    
    public async Task InvokeAsync(HttpListenerContext context, Func<Task> next)
    {
        var request = context.Request;
        var path = request.Url.LocalPath;
        var controllerPath = (path.Substring(1, path.Length - 1)).Substring(0, path.IndexOf('/', 1) - 1);

        BaseController controller = controllerPath switch
        {
            /*"home" => new HomeController { Context = context },
            "article" => new ArticleController { Context = context },
            "llm" => new AIController { Context = context }, */
            "user" => _userController,
            _ => throw new FileNotFoundException()
        };
        
        var result = await controller.HandleRequest(context);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(result.ToString()));
            
    }
}

