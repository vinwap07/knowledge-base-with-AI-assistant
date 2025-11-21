using System.Text.Json;
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
    public async Task<int> CreateArticle(string sessionId, Article article, string role)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        article.PublishDate = DateOnly.FromDateTime(DateTime.Now);
        article.Author = user.Email;
        article.Summary = await CreateSummary(article.Content);
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

    public async Task<Article> GetArticleById(int articleId, string sessionId)
    {
        var article = await _articleRepository.GetById(articleId);
        article.IsLikedByUser = await CheckLike(article.Id, sessionId);
        return article;
    }

    public async Task<List<Article>> SearchArticlesByCategory(string? category, string sessionId)
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
        
        foreach (var article in articles)
        {
            article.IsLikedByUser = await CheckLike(article.Id, sessionId);
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

    public async Task<List<Article>> GetArticlesByAuthor(string authorEmail, string sessionId)
    {
        var articles = await _articleRepository.GetArticleByAuthor(authorEmail);
        foreach (var article in articles)
        {
            article.IsLikedByUser = await CheckLike(article.Id, sessionId);
        }
        return articles;
    }

    public async Task<List<Article>> GetPopularArticles(int count, string sessionId)
    {
        var articles = await _articleRepository.GetArticlesByLikeCount(count);
        foreach (var article in articles)
        {
            article.IsLikedByUser = await CheckLike(article.Id, sessionId);
        }
        return articles;
    }
    
    public async Task<bool> LikeArticle(string sessionId, int articleId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return await _articleRepository.LikeArticle(user.Email, articleId);
    }

    public async Task<bool> RemoveArticleFromLiked(string sessionId, int articleId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return await _articleRepository.RemoveArticleFromFavorite(user.Email, articleId);
    }

    public async Task<List<Article>> GetAllArticles(string sessionId)
    {
        var articles = await _articleRepository.GetAll();
        foreach (var article in articles)
        {
            article.IsLikedByUser = await CheckLike(article.Id, sessionId);
        }
        return articles;
    }

    public async Task<bool> CheckLike(int articleId, string sessionId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        var isLiked = await _articleRepository.CheckLikeFromUser(user.Email, articleId);
        return isLiked;
    }
    
    public async Task<List<Article>> GetFavouriteArticles(string sessionId)
    {
        List<Article> articles = new List<Article>();
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        articles = await _articleRepository.GetAllFavoriteArticles(user.Email);
        foreach (var article in articles)
        {
            article.IsLikedByUser = await CheckLike(article.Id, sessionId);
        }
        return articles;
    }

    private async Task<string> CreateSummary(string articleContent)
    {
        var answer = await OllamaService.SendRequest(articleContent);
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(answer);
        return ollamaResponse.Response;
    }
}