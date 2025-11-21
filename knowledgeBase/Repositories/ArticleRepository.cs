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

    public async Task<bool> CheckLikeFromUser(string userEmail, int articleId)
    {
        var sql = @"SELECT COUNT(*) from UserArticle 
                    WHERE ""User"" = @UserEmail and Article = @ArticleId;";
        var parameters = new Dictionary<string, object>
        {
            { "@UserEmail", userEmail },
            { "ArticleId", articleId }
        };
        var count = (long) await _databaseConnection.ExecuteScalar(sql, parameters);
        return count > 0;
    }
    
    public async Task<List<Article>> GetByCategory(string category)
    {
        var sql = @"select article.*, category.""name"" as categoryName, category.icon as icon 
        from Article JOIN category ON category.slug = article.category 
        WHERE Article.Category = @category";
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
        var sql = @"select article.*, category.""name"" as categoryName, category.icon as icon
        from Article JOIN category ON category.slug = article.category 
        where Id = @Id";
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
        var sql = @"select article.*, category.""name"" as categoryName, category.icon as icon 
        from Article JOIN category ON category.slug = article.category 
        where Title = @title";
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
        var sql = @"select article.*, category.""name"" as categoryName, category.icon as icon 
        from Article JOIN category ON category.slug = article.category";
        var articles = new List<Article>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql);
        while (reader.Read())
        {
            articles.Add(Mapper.MapToArticle(reader));
        }
        
        return articles;
    }

    public async override Task<bool> Create(Article article)
    {
        var createSql = @"BEGIN;
        INSERT INTO Article (Title, Content, Author, PublishDate, Category, Summary, ReadingTime, Description) 
        VALUES (@Title, @Content, @Author, @PublishDate, @Category, @Summary, @ReadingTime, @Description);
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
            ["@ReadingTime"] = article.ReadingTime, 
            ["@Description"] = article.Description
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
        var sql = @"select article.*, category.""name"" as categoryName, category.icon as icon
        from Article JOIN category ON category.slug = article.category
        where Author = @Author";
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
        var sql = @"SELECT article.*, category.""name"" as categoryName, category.icon as icon 
        from Article JOIN category ON category.slug = article.category
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
    
    public async Task<bool> LikeArticle(string userEmail, int articleId)
    {
        var sql = @"BEGIN;
                    INSERT INTO UserArticle (""User"", Article)
                    VALUES (@UserEmail, @ArticleId);
                    UPDATE Article SET LikesCount = LikesCount + 1
                    WHERE Article.id = @ArticleId;
                    COMMIT;";
        var parameters = new Dictionary<string, object>
        {
            ["@UserEmail"] = userEmail,
            ["@ArticleId"] = articleId
        };
        
        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async Task<bool> RemoveArticleFromFavorite(string userEmail, int articleId)
    {
        var sql = @"BEGIN;
                    DELETE FROM UserArticle
                    WHERE ""User"" = @UserEmail AND article = @ArticleId;
                    UPDATE Article SET LikesCount = LikesCount - 1
                    WHERE Article.id = @ArticleId;
                    COMMIT;";
        var parameters = new Dictionary<string, object>
        {
            ["@UserEmail"] = userEmail,
            ["@ArticleId"] = articleId
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }
    
    public async Task<List<Article>> GetAllFavoriteArticles(string userEmail)
    {
        var sql = @"SELECT Article.*, Category.""name"" as categoryName, category.icon as icon
                    from Article JOIN Category ON Category.slug = Article.category
                    JOIN UserArticle ON UserArticle.Article = Article.Id
                    WHERE UserArticle.""User"" = @UserEmail;";
        var parameters = new Dictionary<string, object>
        {
            ["@UserEmail"] = userEmail
        };
        var articles = new List<Article>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        while (reader.Read())
        {
            articles.Add(Mapper.MapToArticle(reader));
        }
        
        return articles;
    }
}