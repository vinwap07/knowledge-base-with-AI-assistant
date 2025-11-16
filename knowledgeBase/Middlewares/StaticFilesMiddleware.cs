using System.Net;

namespace knowledgeBase.Middleware;

public class StaticFilesMiddleware(string root) : IMiddleware
{
    private readonly Dictionary<string, string> _mimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".html", "text/html; charset=utf-8" },
        { ".htm", "text/html; charset=utf-8" },
        { ".css", "text/css; charset=utf-8" },
        { ".js", "application/javascript" },
        { ".json", "application/json" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" },
        { ".svg", "image/svg+xml" },
        { ".ico", "image/x-icon" },
        { ".txt", "text/plain; charset=utf-8" },
        { ".pdf", "application/pdf" }
    };
    public async Task InvokeAsync(HttpContext context, Func<Task> next)
    {
        var request = context.Request;
        var response = context.Response;

        var localPath = request.Url?.LocalPath.TrimStart('/') ?? "";
        if (string.IsNullOrEmpty(localPath) || localPath == "home")
        {
            localPath = "index.html";
        }

        var filePath = Path.Combine(root, localPath);
        if (!File.Exists(filePath))
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            throw new FileNotFoundException();
        }

        var fileInfo = new FileInfo(filePath);
        string extension = fileInfo.Extension.ToLowerInvariant();
        if (_mimeTypes.ContainsKey(extension))
        {
            response.ContentType = _mimeTypes[extension];
        }
        else
        {
            response.ContentType = "application/octet-stream";
        }

        response.ContentLength64 = fileInfo.Length;
        response.StatusCode = (int)HttpStatusCode.OK;
        await SendFileAsync(response, filePath);
    }

    private async Task SendFileAsync(HttpListenerResponse response, string filePath)
    {
        const int bufferSize = 4096;
        byte[] buffer = new byte[bufferSize];
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
        using (var output = response.OutputStream)
        {
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await output.WriteAsync(buffer, 0, bytesRead);
            }
        }
    }

}