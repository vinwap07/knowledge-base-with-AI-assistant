using knowledgeBase.Entities;
using knowledgeBase.View_Models;

namespace knowledgeBase;

public static class DTOMaker
{
    public static List<ArticlePreviewDTO> MapArticles(List<Article> articles)
    {
        var articlePreviews = new List<ArticlePreviewDTO>();
        foreach (var article in articles)
        {
            articlePreviews.Add(new ArticlePreviewDTO()
            {
                Id = article.Id,
                Author = article.Author,
                Title = article.Title,
                Summary = article.Summary,
                PublishDate = article.PublishDate,
                ReadingTime = article.ReadingTime,
                LikesCount = article.LikesCount
            });
        }
        return articlePreviews;
    }

    public static ArticlePreviewDTO MapArticle(Article article)
    {
        return new ArticlePreviewDTO()
        {
            Id = article.Id,
            Author = article.Author,
            Title = article.Title,
            Summary = article.Summary,
            PublishDate = article.PublishDate,
            ReadingTime = article.ReadingTime,
            LikesCount = article.LikesCount
        };
    }

    public static UserProfileDTO MapUser(User user, string role)
    {
        return new UserProfileDTO()
        {
            Email = user.Email,
            Name = user.Name,
            Role = role
        };
    }
}