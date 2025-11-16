using System.Net;
using knowledgeBase;

public class Route
{
    public string Method { get;}
    public string[] PathSegments { get;}
    public string PathTemplate { get; }
    public Func<HttpContext, Dictionary<string, string>, Task> Handler { get; }
    public bool CanUserByUnknown { get; }

    public Route(string method, string pathTemplate,
        Func<HttpContext, Dictionary<string, string>, Task> handler, bool canUserByUnknown = false)
    {
        Method = method;
        PathTemplate = pathTemplate;
        Handler = handler;
        PathSegments = pathTemplate.Split('/')
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
        CanUserByUnknown = canUserByUnknown;
    }

    public bool IsMatch(string method, string path)
    {
        if (Method != method.ToUpper())
        {
            return false;
        }
        
        var requestSegments = path.Split('/')
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
        if (requestSegments.Length != PathSegments.Length)
        {
            return false;
        }

        for (int i = 0; i < PathSegments.Length; i++)
        {
            string templateSegment = PathSegments[i];
            string requestSegment = requestSegments[i];
            if (!IsParameter(templateSegment) &&
                !string.Equals(templateSegment, requestSegment, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        
        return true;
    }

    public Dictionary<string, string> ExtractParameters(string path)
    {
        var parameters = new Dictionary<string, string>();
        var requestSegments = path.Split('/')
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
        for (int i = 0; i < PathSegments.Length; i++)
        {
            string templateSegment = PathSegments[i];
            if (IsParameter(templateSegment))
            {
                string paramName = templateSegment.Trim('{', '}');
                string paramValue = requestSegments[i];
                parameters[paramName] = paramValue;
            }
        }
        
        return parameters;
    }

    private bool IsParameter(string segment)
    {
        return segment.StartsWith("{") && segment.EndsWith("}");
    }
}

public class RouteTable
{
    private readonly List<Route> _routes = new List<Route>();

    public void AddRoute(string method, string pathTemplate,
        Func<HttpContext, Dictionary<string, string>, Task> handler, bool canUserByUnknown = false)
    {
        var route = new Route(method, pathTemplate, handler, canUserByUnknown);
        _routes.Add(route);
    }

    public void Get(string pathTemplate, Func<HttpContext, Dictionary<string, string>, Task> handler, bool canUserByUnknown = false)
    {
        AddRoute("GET", pathTemplate, handler, canUserByUnknown);
    }

    public void Post(string pathTemplate, Func<HttpContext, Dictionary<string, string>, Task> handler, bool canUserByUnknown = false)
    {
        AddRoute("POST", pathTemplate, handler, canUserByUnknown);
    }

    public void Put(string pathTemplate, Func<HttpContext, Dictionary<string, string>, Task> handler, bool canUserByUnknown = false)
    {
        AddRoute("PUT", pathTemplate, handler, canUserByUnknown);
    }

    public void Delete(string pathTemplate, Func<HttpContext, Dictionary<string, string>, Task> handler, bool canUserByUnknown = false)
    {
        AddRoute("DELETE", pathTemplate, handler, canUserByUnknown);
    }

    public Route? FindRoute(string method, string path)
    {
        return _routes.Find(route => route.IsMatch(method, path));
    }
}