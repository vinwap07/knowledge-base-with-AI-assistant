using System.Net;
using System.Web;

namespace knowledgeBase;

public static class FormParser
{
    public static async Task<Dictionary<string, string>> ParseAsync(HttpListenerRequest request)
    {
        if (request.HttpMethod != "POST" || !request.HasEntityBody)
        {
            return new Dictionary<string, string>();
        }
        
        long contentLength = request.ContentLength64;
        
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();
        
        var formData = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(body))
        {
            return formData;
        }
        
        var pairs = body.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2)
            {
                string key = HttpUtility.UrlDecode(parts[0]);
                string value = HttpUtility.UrlDecode(parts[1]);
                formData.Add(key, value);
            }
        }
        
        return formData;
    }
}