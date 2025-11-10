using System.Net;
using System.Text.Json;

namespace knowledgeBase.Controllers;

public abstract class BaseController
{
    public abstract Task<string> HandleRequest(HttpListenerContext context);
    protected string ToJson(object data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        return JsonSerializer.Serialize(data, options);
    }
    protected void Redirect(HttpListenerContext context, string url)
    {
        context.Response.Redirect(url);
    }
    
    protected async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
    {
        try
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read request body", ex);
        }
    }
}