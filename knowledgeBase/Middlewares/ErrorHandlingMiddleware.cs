using System.Net;
using System.Text;

namespace knowledgeBase.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly string _logErrorFilePath;
    public async Task InvokeAsync(HttpContext context, Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        var (statusCode, message) = MapExceptionToHttpResponse(exception);
        
        response.StatusCode = (int)statusCode;
        string errorPage = PageCreator.CreateErrorPage(statusCode, message);
        
        var buffer = Encoding.UTF8.GetBytes(errorPage);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html; charset=utf-8";
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        
        await LogError(context, exception, statusCode);
    }
    
    private (HttpStatusCode statusCode, string message) MapExceptionToHttpResponse(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Доступ запрещен. Пожалуйста, авторизуйтесь."),
            FileNotFoundException => (HttpStatusCode.NotFound, "Страница не найдена."),
            ArgumentException => (HttpStatusCode.BadRequest, "Неверный запрос. Проверьте введенные данные."),
            InvalidOperationException => (HttpStatusCode.Conflict, "Операция не может быть выполнена."),
            TimeoutException => (HttpStatusCode.RequestTimeout, "Время ожидания истекло."),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Функционал находится в разработке."),
            _ => (HttpStatusCode.InternalServerError, "Произошла внутренняя ошибка сервера.")
        };
    }
    private async Task LogError(HttpContext context, Exception exception, HttpStatusCode statusCode)
    {
        var request = context.Request;
        string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                            $"ERROR: {request.HttpMethod} {request.Url} " +
                            $"Status: {(int)statusCode} " +
                            $"Exception: {exception.GetType().Name} " +
                            $"Message: {exception.Message} " +
                            $"User: {request.RemoteEndPoint?.Address.ToString() ?? "Unknown"} " +
                            $"Stack: {exception.StackTrace?.Split('\n')[0]}\n";
        try
        {
            await File.AppendAllTextAsync(_logErrorFilePath, logMessage);
        }
        catch
        {
            Console.WriteLine(logMessage);
        }
    }
}