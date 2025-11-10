using System.Net;

namespace knowledgeBase;

public class Route
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Func<HttpListenerContext, Dictionary<string, string>, Task> Handler { get; set; } = null!;
}

public class RouteMatch
{
    public Route Route { get; set; } = null!;
    public Dictionary<string, string> Parameters { get; set; } = new();
}

public class RouteTable
{
    private readonly List<Route> _routes = new();

    public void AddRoute(string method, string path,
        Func<HttpListenerContext, Dictionary<string, string>, Task> handler)
    {
        _routes.Add(new Route()
        {
            Method = method.ToUpper(),
            Path = path,
            Handler = handler
        });
    }
    
    public void Get(string path, Func<HttpListenerContext, Dictionary<string, string>, Task> handler)
        => AddRoute("GET", path, handler);

    public void Post(string path, Func<HttpListenerContext, Dictionary<string, string>, Task> handler)
        => AddRoute("POST", path, handler);

    public void Delete(string path, Func<HttpListenerContext, Dictionary<string, string>, Task> handler)
        => AddRoute("DELETE", path, handler);

    public RouteMatch? FindMatch(string method, string path)
    {
        var upperMethod = method.ToUpper();
        foreach (var route in _routes)
        {
            if (route.Method != upperMethod) continue;

            var parameters = MatchPath(route.Path, path);
            if (parameters != null)
            {
                return new RouteMatch
                {
                    Route = route,
                    Parameters = parameters
                };
            }
        }

        return null;
    }

    private Dictionary<string, string>? MatchPath(string routePath, string requestPath)
    {
        var routeParts = routePath.Split('/');
        var requestParts = requestPath.Split('/');

        if (routeParts.Length != requestParts.Length)
        {
            return null;
        }
        
        var parameters = new Dictionary<string, string>();
        for (int i = 0; i < routeParts.Length; i++)
        {
            if (routeParts[i].StartsWith("{") && routeParts[i].EndsWith("}"))
            {
                var paramName = routeParts[i].Trim('{', '}');
                parameters[paramName] = requestParts[i];
            }
            else if (routeParts[i] != requestParts[i])
            {
                return null;
            }
        }
        
        return parameters;
    }
}