using System.Data;
using knowledgeBase.Entities;
using knowledgeBase.DataBase;

namespace knowledgeBase.Repositories;

public class ArticleRepository : BaseRepository<Article, int>
{
    private readonly IDatabaseConnection _databaseConnection;
    
    public ArticleRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }
    
    public async Task<List<Article>> GetByCategory(string category)
    {
        var sql = @"select * from Article WHERE Category = @category";
        var parameters = new Dictionary<string, object>
        {
            { "@category", category }
        };
        
        var articles = new List<Article>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            articles.Add(Mapper.MapToArticle(reader));
        }
        
        return articles;
    }
    
    public async override Task<Article> GetById(int id)
    {
        var sql = @"select * from Article where Id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = id
        };
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return Mapper.MapToArticle(reader);
        }
        
        return null;
    }

    public async Task<List<Article>> GetByTitle(string title)
    {
        var sql = @"select * from Article where Title = @title";
        var parameters = new Dictionary<string, object>
        {
            ["@title"] = title
        };
        var articles = new List<Article>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            articles.Add(Mapper.MapToArticle(reader));
        }
        return articles;
    }

    public async override Task<List<Article>> GetAll()
    {
        var sql = @"select * from Article";
        var articles = new List<Article>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql);
        if (reader.Read())
        {
            articles.Add(Mapper.MapToArticle(reader));
        }
        
        return articles;
    }

    public async override Task<bool> Create(Article article)
    {
        var createSql = @"BEGIN;
        INSERT INTO Article (Title, Content, Author, PublishDate, Category, Summary, ReadingTime) 
        VALUES (@Title, @Content, @Author, @PublishDate, @Category, @Summary, @ReadingTime);
        UPDATE Category SET articlescount = articlescount + 1
        WHERE slug = @Category;
        COMMIT;";
        var createParameters = new Dictionary<string, object>
        {
            ["@Title"] = article.Title,
            ["@Content"] = article.Content,
            ["@Author"] = article.Author,
            ["@PublishDate"] = article.PublishDate,  
            ["@Category"] = article.Category,
            ["@Summary"] = article.Summary,
            ["@ReadingTime"] = article.ReadingTime
        };

        return await _databaseConnection.ExecuteNonQuery(createSql, createParameters) > 0;
    }
    
    public async override Task<bool> Update(Article article)
    {
        var sql = @"update Article
                set Title = @Title, Content = @Content, Author = @Author, PublishDate = @PublishDate, Summary = @Summary, Category = @Category
                where Id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = article.Id,
            ["@Title"] = article.Title,
            ["@Content"] = article.Content,
            ["@Author"] = article.Author,
            ["@PublishDate"] = article.PublishDate,
            ["@Category"] = article.Category,
            ["@Summary"] = article.Summary
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async override Task<bool> Delete(int id)
    {
        var sql = @"delete from Article where Id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = id,
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async Task<long> GetArticleLikesCountById(int articleId)
    {
        var sql = @"select count(*) from UserArticle where Article = @ArticleId";
        var parameters = new Dictionary<string, object>
        {
            ["@ArticleId"] = articleId
        };
        
        return (long)(await _databaseConnection.ExecuteScalar(sql, parameters)); 
    }

    public async Task<List<Article>> GetArticleByAuthor(string authorEmail)
    {
        var sql = @"select * from Article where Author = @Author";
        var parameters = new Dictionary<string, object>
        {
            ["@Author"] = authorEmail
        };
        
        var articles = new List<Article>();
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        while (reader.Read())
        {
            articles.Add(Mapper.MapToArticle(reader));
        }
        
        return articles;
    }

    public async Task<List<Article>> GetArticlesByLikeCount(int count)
    {
        var sql = @"SELECT * FROM Article 
                    ORDER BY LikesCount DESC
                    LIMIT @Count";
        var parameters = new Dictionary<string, object>
        {
            ["@Count"] = count
        };
        
        var articles = new List<Article>();
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        while (reader.Read())
        {
            articles.Add(Mapper.MapToArticle(reader));
        }
        
        return articles;
    }
    
    public async Task<bool> AddArticleToFavorite(string userEmail, int articleId)
    {
        var sql = @"INSERT INTO UserArticle (""User"", Article)
                    VALUES (@UserEmail, @ArticleId);
                    UPDATE Article SET LikeCount = LikeCount + 1
                    WHERE ArticleId = @ArticleId;";
        var parameters = new Dictionary<string, object>
        {
            ["@UserEmail"] = userEmail,
            ["@ArticleId"] = articleId
        };
        
        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async Task<bool> RemoveArticleFromFavorite(string userEmail, int articleId)
    {
        var sql = @"DELETE FROM UserArticle
                    WHERE user = @UserEmail AND article = @ArticleId;
                    UPDATE Article SET LikeCount = LikeCount - 1
                    WHERE ArticleId = @ArticleId;";
        var parameters = new Dictionary<string, object>
        {
            ["@UserEmail"] = userEmail,
            ["@ArticleId"] = articleId
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }
}