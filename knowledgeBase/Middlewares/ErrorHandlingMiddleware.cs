using System.Net;
using System.Text;

namespace knowledgeBase.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly string _logErrorFilePath;
    public async Task InvokeAsync(HttpListenerContext context, Func<Task> next)
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

    private async Task HandleExceptionAsync(HttpListenerContext context, Exception exception)
    {
        var response = context.Response;
        var (statusCode, message) = MapExceptionToHttpResponse(exception);
        
        response.StatusCode = (int)statusCode;
        string errorPage = GenerateErrorPage(statusCode, message, exception);
        
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
            FileNotFoundException => (HttpStatusCode.NotFound, "Странциа не найдена."),
            ArgumentException => (HttpStatusCode.BadRequest, "Неверный запрос. Проверьте введенные данные."),
            InvalidOperationException => (HttpStatusCode.Conflict, "Операция не может быть выполнена."),
            TimeoutException => (HttpStatusCode.RequestTimeout, "Время ожидания истекло."),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Функционал находится в разработке."),
            _ => (HttpStatusCode.InternalServerError, "Произошла внутренняя ошибка сервера.")
        };
    }

    private string GenerateErrorPage(HttpStatusCode statusCode, string message, Exception exception)
    {
        var statusCodeNumber = (int)statusCode;
        var statusCodeText = statusCode.ToString();
        
        string html = $@"
                <!DOCTYPE html>
                <html lang='ru'>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Ошибка {statusCodeNumber} - {statusCodeText}</title>
                </head>
                <body>
                    <div class='error-container'>
                        <div class='error-code'>{statusCodeNumber}</div>
                        <h1 class='error-title'>{statusCodeText}</h1>
                        <div class='error-message'>{message}</div>
                        
                        <a href='/' class='home-button'>Вернуться на главную</a>
                        <a href='/contact' class='home-button' style='background: #556cd6; margin-left: 10px;'>Сообщить об ошибке</a>
                    </div>
                </body>
                </html>";
        return html;
    }

    private async Task LogError(HttpListenerContext context, Exception exception, HttpStatusCode statusCode)
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