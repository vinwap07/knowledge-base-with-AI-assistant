using knowledgeBase.Entities;
using knowledgeBase.Repositories;

namespace knowledgeBase.Services;

public class ArticleService
{
    private ArticleRepository _articleRepository;
    private SessionRepository _sessionRepository;

    public ArticleService(ArticleRepository articleRepository, SessionRepository sessionRepository)
    {
        _articleRepository = articleRepository;
        _sessionRepository = sessionRepository;
    }
    public async Task<int> CreateArticle(Article article, string role)
    {
        // TODO: валидация статьи
        var isArticleAdded = await _articleRepository.Create(article);
        var articles = await _articleRepository.GetByTitle(article.Title);
        int articleId = -100;
        
        if (isArticleAdded)
        {
            foreach (var art in articles)
            {
                if (art.Title == article.Title)
                {
                    articleId = art.Id;
                    break;
                }
            }
        }
        
        return articleId;
    }

    public async Task<Article> GetArticleById(int articleId)
    {
        var article = await _articleRepository.GetById(articleId);
        return article;
    }

    public async Task<List<Article>> SearchArticlesByCategory(string? category)
    {
        var articles = new List<Article>();
        // TODO: переделать лист Article в лист ArticleViewModel 
        if (category != null)
        {
            articles = await _articleRepository.GetByCategory(category);
        }
        else
        {
            articles = await _articleRepository.GetAll();
        }

        return articles;
    }

    public async Task<bool> DeleteArticle(Article article, string sessionId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        var isDeleted = false;
        if (article.Author == user.Email)
        {
            isDeleted = await _articleRepository.Delete(article.Id); 
        }
        
        return isDeleted;
    }

    public async Task<long> GetArticleLikesCount(int articleId)
    {
        var likesCount = await _articleRepository.GetArticleLikesCountById(articleId);
        return likesCount;
    }
}