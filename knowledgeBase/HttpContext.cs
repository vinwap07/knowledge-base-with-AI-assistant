using System.Net;

namespace knowledgeBase;

public class HttpContext
{
    public HttpListenerContext Context { get; set; }
    public string Role { get; set; }

    public HttpContext(HttpListenerContext context, string role)
    {
        Context = context;
        Role = role;
    }
}