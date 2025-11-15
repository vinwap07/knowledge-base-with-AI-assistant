using System.Net;
using System.Text;
using System.Text.Json;

namespace knowledgeBase.Controllers;

public abstract class BaseController
{
    protected string ToJson(object data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
    
    protected async Task<T> FromJsonBodyAsync<T>(HttpListenerRequest request)
    {
        try
        {
            string json = await ReadRequestBodyAsync(request);
            
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("Request body is empty");
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<T>(json, options) ?? 
                   throw new JsonException("Failed to deserialize JSON");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON format", ex);
        }
    }
    
    protected async Task SendJsonAsync(HttpListenerResponse response, object data, int statusCode = 200)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json; charset=utf-8";
        
        string json = ToJson(data);
        await WriteResponseAsync(response, json);
    }
    
    protected async Task SendTextAsync(HttpListenerResponse response, string text, int statusCode = 200)
    {
        response.StatusCode = statusCode;
        response.ContentType = "text/plain; charset=utf-8";
        
        await WriteResponseAsync(response, text);
    }
    
    protected async Task WriteResponseAsync(HttpListenerResponse response, string content)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buffer.Length;
            
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        catch (ObjectDisposedException)
        {
            // Поток уже закрыт - игнорируем
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing response: {ex.Message}");
            throw;
        }
    }
}