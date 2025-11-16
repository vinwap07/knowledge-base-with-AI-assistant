using System.Data;
using knowledgeBase.Entities;
using knowledgeBase.DataBase;

namespace knowledgeBase.Repositories;

public class CategoryRepository : BaseRepository<Category, string>
{
    private readonly IDatabaseConnection _databaseConnection;
    
    public CategoryRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }
    
    public async override Task<Category> GetById(string name)
    {
        var sql = @"select * from Category where Name = @Name";
        var parameters = new Dictionary<string, object>
        {
            ["@Name"] = name
        };
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return Mapper.MapToCategory(reader);
        }
        
        return null;
    }

    public async override Task<List<Category>> GetAll()
    {
        var sql = @"select * from Category";
        var questionLogs = new List<Category>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql);
        while (reader.Read())
        {
            questionLogs.Add(Mapper.MapToCategory(reader));
        }
        
        return questionLogs;
    }

    public async override Task<bool> Create(Category category)
    {
        // TODO: переделать с новыми полями
        var createSql = @"insert into Category (Name, Slug) 
                values (@Name, @Slug);";
        var createParameters = new Dictionary<string, object>
        {
            ["@Name"] = category.Name,
            ["@Slug"] = category.Slug
        };

        return await _databaseConnection.ExecuteNonQuery(createSql, createParameters) > 0;
    }

    public async override Task<bool> Update(Category category)
    {
        // TODO: переделать с новыми полями
        var sql = @"update Category
                set Slug = @Slug
                where Name = @Name";
        var parameters = new Dictionary<string, object>
        {
            ["@Name"] = category.Name,
            ["@Slug"] = category.Slug
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async override Task<bool> Delete(string name)
    {
        var sql = @"delete from Category where Name = @Name";
        var parameters = new Dictionary<string, object>
        {
            ["@Name"] = name,
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }
}

