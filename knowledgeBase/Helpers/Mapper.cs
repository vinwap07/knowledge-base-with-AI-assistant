using System.Data;
using knowledgeBase.Entities;
namespace knowledgeBase;

public static class Mapper
{
    public static User MapToUser(IDataReader reader)
    {
        return new User
        {
            Email = (string)reader["Email"],
            Name = (string)reader["Name"],
            Password = (string)reader["Password"],
            RoleId = (int)reader["RoleId"]
        };
    }

    public static Article MapToArticle(IDataReader reader)
    {
        return new Article()
        {
            Id = (int)reader["Id"],
            Title = (string)reader["Title"],
            Summary = (string)reader["Summary"],
            Description = (string)reader["Description"],
            Category = (string)reader["CategoryName"],
            Content = (string)reader["Content"],
            Author = (string)reader["Author"],
            PublishDate = (DateOnly)reader["PublishDate"],
            ReadingTime = (int)reader["ReadingTime"], 
            LikesCount = (int)reader["LikesCount"],
            Icon = (string)reader["Icon"],
        };
    }

    public static Session MapToSession(IDataReader reader)
    {
        return new Session()
        {
            SesisonId = (string)reader["sessionid"],
            UserEmail = (string)reader["useremail"],
            EndTime = (DateTime)reader["endtime"],
        };
    }

    public static Category MapToCategory(IDataReader reader)
    {
        return new Category()
        {
            Name = (string)reader["Name"],
            Slug = (string)reader["Slug"],
            ArticlesCount = (int)reader["ArticlesCount"],
            Icon = (string)reader["Icon"],
            Description = (string)reader["Description"],
        };
    }

    public static UserArticle MapToUserArticle(IDataReader reader)
    {
        return new UserArticle()
        {
            User = (string)reader["User"],
            Article = (int)reader["Article"],
        };
    }
    
    
}