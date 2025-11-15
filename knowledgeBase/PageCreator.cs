using System.Net;
using knowledgeBase.Entities;

namespace knowledgeBase;

public static class PageCreator
{
    public static string CreateErrorPage(HttpStatusCode statusCode, string message)
    {
        var statusCodeNumber = (int)statusCode;
        var statusCodeText = statusCode.ToString();
        
        string html = $@"
        <!DOCTYPE html>
        <html lang='ru'>
        <head>
            <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
            <meta name='viewport' content='width=device-width, initial-scale=1'>
            <title>Ошибка {statusCodeNumber} - {statusCodeText}</title>
            <link rel=""stylesheet"" type=""text/css"" href=""http://localhost:5000/styles/error.css""/>
            <script defer src=""http://localhost:5000/js/main.js""></script>
        </head>
            <body>
                <div class='error-container'>
                <div class='error-code'>{statusCodeNumber}</div>
                <h1 class='error-title'>{statusCodeText}</h1>
                <div class='error-message'>{message}</div>
                    <a href='http://localhost:5000/index.html' class='home-button'>Вернуться на главную</a>
                </div>
            </body>
        </html>";
        return html;
    }

    public static string GetArticlePage(Article article)
    {
        var page = $@"<!DOCTYPE html>
<html lang='ru'>
<head>
    <meta charset=""UTF-8"">
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Статья - KnowBase</title>
    <link rel=""stylesheet"" type=""text/css"" href=""http://localhost:5000/styles/main.css""/>
    <link rel=""stylesheet"" type=""text/css"" href=""http://localhost:5000/styles/article.css""/>
    <script defer src=""http://localhost:5000/js/header-auth-checker.js""></script>
    <script defer src=""http://localhost:5000/js/script.js""></script>
</head>
<body>
<header>
    <div class=""container"">
        <nav class=""navbar"">
            <a href=""index.html"" class=""logo"">Know<span>Base</span></a>
            <ul class=""nav-links"">
                <li><a href=""http://localhost:5000/index.html"">Главная</a></li>
                <li><a id=""get-categories-btn"">Категории</a></li>
                <li><a id=""get-articles-btn"">Статьи</a></li>
            </ul>
            <div class=""auth-buttons"" id=""authButtons"">
                <!-- Кнопки будут динамически обновляться -->
                <a href=""http://localhost:5000/login.html"" class=""btn btn-outline"" id=""loginBtn"">Войти</a>
                <a href=""http://localhost:5000/register.html"" class=""btn btn-primary"" id=""registerBtn"">Регистрация</a>
            </div>
            <div class=""user-menu"" id=""userMenu"" style=""display: none;"">
                <span class=""username"" id=""userName"">Пользователь</span>
                <div class=""user-avatar"" id=""userAvatar"">?</div>
            </div>
        </nav>
    </div>
</header>

<div class=""article-container"">
    <div class=""article-header"">
        <h1 class=""article-title"" id=""articleTitle"">{article.Title}</h1>
        <div class=""article-meta"">
            <div class=""meta-item"">
                <span class=""meta-label"">Автор:</span>
                <span class=""meta-value"" id=""articleAuthor"">{article.Author}</span>
            </div>
            <div class=""meta-item"">
                <span class=""meta-label"">Категория:</span>
                <span class=""meta-value"" id=""articleCategory"">{article.Category}</span>
            </div>
            <div class=""meta-item"">
                <span class=""meta-label"">Опубликовано:</span>
                <span class=""meta-value"" id=""articlePublishDate"">{article.PublishDate}</span>
            </div>
            <div class=""meta-item"">
                <span class=""meta-label"">Время чтения:</span>
                <span class=""meta-value"" id=""articleReadingTime"">{article.ReadingTime}</span>
            </div>
        </div>
    </div>

    <div class=""article-summary"" id=""articleSummary"">
        {article.Summary}
    </div>

    <div class=""article-content"" id=""articleContent"">
        {article.Content}
    </div>

    <div class=""article-actions"">
        <button class=""btn btn-outline"" onclick=""history.back()"">← Назад</button>
        <button class=""btn btn-primary"" id=""likeBtn"">❤️ Нравится</button>
    </div>
</div>
</body>
</html>";
        return page;
    }
}