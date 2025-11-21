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
            articlePreviews.Add(MapArticle(article));
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
            Description = article.Description,
            PublishDate = article.PublishDate,
            ReadingTime = article.ReadingTime,
            LikesCount = article.LikesCount, 
            Category = article.Category, 
            IsLikedByUser = article.IsLikedByUser,
            Icon = article.Icon
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

    public static List<CategoryDTO> MapCategories(List<Category> categories)
    {
        var categoriesDTO = new List<CategoryDTO>();
        foreach (var category in categories)
        {
            categoriesDTO.Add(new CategoryDTO()
            {
                Name = category.Name,
                Slug = category.Slug
            });
        }
        
        return categoriesDTO;
    }
}