using System.Data;
using knowledgeBase.Entities;
using knowledgeBase.DataBase;

namespace knowledgeBase.Repositories;

public class UserRepository: BaseRepository<User, string>
{
    private readonly IDatabaseConnection _databaseConnection;

    public UserRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }
    
    public async override Task<User> GetById(string email)
    {
        var sql = @"select * from ""User"" where Email = @Email";
        var parameters = new Dictionary<string, object>
        {
            ["@Email"] = email
        };
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return Mapper.MapToUser(reader);
        }
        
        return new User();
    }

    public async override Task<List<User>> GetAll()
    {
        var sql = @"select * from ""User""";
        var users = new List<User>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql);
        while (reader.Read())
        {
            users.Add(Mapper.MapToUser(reader));
        }
        
        return users;
    }

    public async override Task<bool> Create(User user)
    {
        var createSql = @"insert into ""User"" (""name"", Email, Password, RoleId) 
        values (@Name, @Email, @Password, @RoleId);";
        var createParameters = new Dictionary<string, object>
        {
            ["@Email"] = user.Email,
            ["@Name"] = user.Name,
            ["@Password"] = user.Password,
            ["@RoleId"] = user.RoleId
        };

        return await _databaseConnection.ExecuteNonQuery(createSql, createParameters) > 0;
    }

    public async override Task<bool> Update(User user)
    {
        var sql = @"update ""User""
                set Name = @Name, Password = @Password, RoleId = @RoleId 
                where Email = @Email";
        var parameters = new Dictionary<string, object>
        {
            ["@Email"] = user.Email,
            ["@Name"] = user.Name,
            ["@Password"] = user.Password,
            ["@RoleId"] = user.RoleId
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async override Task<bool> Delete(string id)
    {
        var sql = @"delete from ""User"" where Id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = id
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }
    
    public async Task<string> GetRoleByRoleId(int id)
    {
        var sql = @"select name from ""Role"" where id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = id
        };
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return (string)reader["Name"];
        }
        
        return "unknown";
    }

    public async Task<string> GetRoleByEmail(string email)
    {
        var sql = @"SELECT ""Name"" from ""Role"" 
                    JOIN ""User"" on ""Role"".Id = ""User"".RoleId
                    where Email = @Email ";
        var parameters = new Dictionary<string, object>
        {
            ["@Email"] = email
        };
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return (string)reader["Name"];
        }
        
        return "unknown";
    }

    public async Task<List<Article>> GetAllFavoriteArticles(string userEmail)
    {
        var sql = @"SELECT Article.*
                    FROM Article JOIN UserArticle ON UserArticle.Article = Article.Id
                    WHERE UserArticle.""User"" = @UserEmail";
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