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
        var likeBtnClass = article.IsLikedByUser ? "like-btn liked" : "like-btn";
        var readingTime = FormateReadingTime(article.ReadingTime);
        var page = $@"<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{{article.Title}} - KnowBase</title>
  <link rel=""stylesheet"" type=""text/css"" href=""http://localhost:5000/styles/article.css""/>
  <link rel=""stylesheet"" type=""text/css"" href=""http://localhost:5000/styles/main.css""/>
  <script defer src=""http://localhost:5000/js/load-components.js""></script>
  <script defer src=""http://localhost:5000/js/header-auth-checker.js""></script>
  <script defer src=""http://localhost:5000/js/article.js""></script>
</head>
<body>
<div id=""header""></div>
<div id=""article-container"" class=""article-container"">
        <div class=""article-content"">
            <h2>{article.Title}</h2>
            <div class=""modal-body"">
                <div class=""article-info"">
                <div class=""article-category"">{article.Category}</div>
                <p class=""article-summary"">{article.Description}</p>
                
                <div class=""article-meta"" id=""article-meta"">
                    <div class=""article-author"" id=""concrete-article-author"">{article.Author}</div>
                    <span class=""article-stat"" id=""concrete-article-stat"">📅 {article.PublishDate}</span>
                    <span class=""article-stat"" id=""concrete-article-stat"">⏱️ {readingTime}</span>
                    <button class=""{likeBtnClass}"" id=""like-btn1""
                            data-article-id=""{article.Id}""
                            data-likes-count=""{article.LikesCount}""
                            data-is-liked=""{article.IsLikedByUser}"">
                        {(article.IsLikedByUser? "💖" : "❤️")} {article.LikesCount}
                    </button>
                </div>
             </div>
             <button class=""showSummaryBtn"" id=""showSummaryBtn"">🤖 AI-саммари</button>
                <div class=""article-content"">{article.Content}</div>
                <div class=""article-actions"">
                    <a href=""http://localhost:5000/articles.html"" class=""read-more"">
                        Ко всем статьям →
                    </a>
                    <button class=""{likeBtnClass}"" id=""like-btn2""
                            data-article-id=""{article.Id}""
                            data-likes-count=""{article.LikesCount}""
                            data-is-liked=""{article.IsLikedByUser}"">
                        {(article.IsLikedByUser? "💖" : "❤️")} {article.LikesCount}
                    </button>
                </div>
            </div>
            </div>
        </div>
    </div>

<div id=""summaryModal"" class=""modal"">
    <div class=""modal-content ai-summary-modal"">
        <span class=""close"" id=""closeModalBtn"">&times;</span>
        
        <div class=""modal-header"">
            <div class=""ai-icon"">🤖</div>
            <h2>AI-резюме статьи</h2>
        </div>
        
        <div class=""modal-body"">
            <div class=""ai-summary-content"">
                {article.Summary}
            </div>

            <div class=""ai-disclaimer"">
                <p>⚠️ Это автоматически сгенерированное резюме. Для полного понимания рекомендуется прочитать оригинальную статью.</p>
            </div>
        </div>
        
        <div class=""modal-footer"">
            <button class=""btn-secondary"" onclick=""closeAISummary()"">Закрыть</button>
            <button class=""btn-secondary"" onclick=""copySummary()"">Копировать саммари</button>
        </div>
    </div>
</div>
</body>
</html>";
        
        return page;
    }

    private static string FormateReadingTime(int minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes} мин";
        }
        
        var hours = minutes / 60;
        var minute = minutes % 60;
        if (minute == 0)
        {
            return $"{hours}ч";
        }

        return $"{hours}ч {minute}мин";
    }
}