using System.Net;

namespace knowledgeBase.Middleware;

public class MiddlewarePipeline
{
    private readonly List<IMiddleware> _middlewares = new();
    
    public void Use<TMiddleware>() where TMiddleware : IMiddleware, new()
    {
        _middlewares.Add(new TMiddleware());
    }
    
    public void Use(IMiddleware middleware)
    {
        _middlewares.Add(middleware);
    }

    public async Task ExecuteAsync(HttpContext myContext)
    {
        int index = 0;
        Func<Task> next = null;

        next = async () =>
        {
            if (index < _middlewares.Count)
            {
                var middleware = _middlewares[index++];

                await middleware.InvokeAsync(myContext, next);
            }
        };
        
        await next();
    }
}