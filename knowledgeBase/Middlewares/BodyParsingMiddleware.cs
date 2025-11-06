using System.Net;
using System.Web;

namespace knowledgeBase.Middleware;

public class BodyParsingMiddleware : IMiddleware
{
    private static readonly Dictionary<HttpListenerContext, Dictionary<string, string>> _formDataStore = new();
    
    public async Task InvokeAsync(HttpListenerContext context, Func<Task> next)
    {
        var request = context.Request;
        if (request.HttpMethod == "POST" && request.HasEntityBody)
        {
            try
            {
                var fromData = await ParseFormDataAsync(request);
                _formDataStore[context] = fromData;
            }
            catch (Exception ex)
            {
                throw new FormDataParseException("Failed to parse form data", ex);
            }
        }

        try
        {
            await next();
        }
        finally
        {
            _formDataStore.Remove(context);
        }
    }

    private async Task<Dictionary<string, string>> ParseFormDataAsync(HttpListenerRequest request)
    {
        var formData = new Dictionary<string, string>();
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(body))
        {
            return formData;
        }
        
        var pairs = body.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2)
            {
                string name = HttpUtility.UrlDecode(parts[0]);
                string value = HttpUtility.UrlDecode(parts[1]);
                
                formData[name] = value;
            }
        }
        
        return formData;
    }
}

public class FormDataParseException : Exception
{
    public FormDataParseException(string message) : base(message) { }
    public FormDataParseException(string message, Exception inner) : base(message, inner) { }
}