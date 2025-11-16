using System.Net;

namespace knowledgeBase.Middleware;

public class LoggingMiddleware : IMiddleware
{
    private readonly string _logFilePath;

    public LoggingMiddleware(string logFilePath = "logs.txt")
    {
        _logFilePath = logFilePath;
    }
    
    public async Task InvokeAsync(HttpContext context, Func<Task> next)
    {
        var startTime = DateTime.UtcNow;
        var request = context.Request;
        var response = context.Response;
        
        var logMessage = $"[{startTime:yyyy-MM-dd HH:mm:ss}] " +
                        $"INCOMING: {request.HttpMethod} {request.Url} " +
                        $"From: {GetClientInfo(request)} " +
                        $"UserAgent: {request.UserAgent ?? "Unknown"}\n";

        await WriteToLog(logMessage);

        try
        {
            await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            logMessage = $"[{endTime:yyyy-MM-dd HH:mm:ss}] " +
                         $"SUCCESS: {request.HttpMethod} {request.Url} " +
                         $"Status: {response.StatusCode} " +
                         $"Duration: {duration.TotalMilliseconds}ms " +
                         $"Size: {GetResponseSize(response)} bytes\n";

            await WriteToLog(logMessage);
        }
        catch (Exception ex)
        {
            var errorTime = DateTime.UtcNow;
            var duration = errorTime - startTime;
            
            logMessage = $"[{errorTime:yyyy-MM-dd HH:mm:ss}] " +
                         $"ERROR: {request.HttpMethod} {request.Url} " +
                         $"Exception: {ex.GetType().Name} - {ex.Message} " +
                         $"Duration: {duration.TotalMilliseconds}ms\n";
            
            await WriteToLog(logMessage);
            
            throw;
        }
    }

    private string GetClientInfo(HttpListenerRequest request)
    {
        var clientIP = request.RemoteEndPoint.Address.ToString();
        var port = request.RemoteEndPoint.Port;
        return $"{clientIP}:{port}";
    }
    
    private string GetResponseSize(HttpListenerResponse response)
    {
        try
        {
            return response.ContentLength64 > 0 ? response.ContentLength64.ToString() : "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private async Task WriteToLog(string message)
    {
        try
        {
            await File.AppendAllTextAsync(_logFilePath, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LOG ERROR: Failed to write to log file: {ex.Message}");
            Console.Write(message);
        }
    }
}