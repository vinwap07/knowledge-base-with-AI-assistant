using knowledgeBase.Entities;
using knowledgeBase.Repositories;
using knowledgeBase.View_Models;

namespace knowledgeBase.Services;

public class ArticleService
{
    private ArticleRepository _articleRepository;
    private SessionRepository _sessionRepository;
    private UserRepository _userRepository;

    public ArticleService(ArticleRepository articleRepository, SessionRepository sessionRepository, UserRepository userRepository)
    {
        _articleRepository = articleRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
    }
    public async Task<int> CreateArticle(Article article, User user, string role)
    {
        article.PublishDate = DateOnly.FromDateTime(DateTime.Now);
        article.Author = user.Email;
        if (role == "user")
        {
            throw new UnauthorizedAccessException();
        }
        
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
        return await _articleRepository.GetById(articleId);
    }

    public async Task<List<Article>> SearchArticlesByCategory(string? category)
    {
        var articles = new List<Article>();
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
    // TODO: переделать подсчет лайков 2 операции - добавление лайка в таблицу лайков и увеличение количества лайков как атрибут статьи
    public async Task<long> GetArticleLikesCount(int articleId)
    {
        return await _articleRepository.GetArticleLikesCountById(articleId);
    }

    public async Task<List<Article>> GetArticlesByAuthor(string authorEmail)
    {
        return await _articleRepository.GetArticleByAuthor(authorEmail);
    }

    public async Task<List<Article>> GetPopularArticles(int count)
    {
        return await _articleRepository.GetArticlesByLikeCount(count);
    }
    
    public async Task<bool> AddArticleToFavorite(string sessionId, int articleId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return await _articleRepository.AddArticleToFavorite(user.Email, articleId);
    }

    public async Task<bool> RemoveArticleFromFavorite(string sessionId, int articleId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return await _articleRepository.RemoveArticleFromFavorite(user.Email, articleId);
    }
}