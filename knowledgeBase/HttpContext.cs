using System.Net;

namespace knowledgeBase;

public class HttpContext
{
    public HttpListenerResponse Response { get; set; }
    public HttpListenerRequest Request { get; set; }
    public string Role { get; set; }

    public HttpContext(HttpListenerContext context, string role)
    {
        Request = context.Request;
        Response = context.Response;
        Role = role;
    }
}