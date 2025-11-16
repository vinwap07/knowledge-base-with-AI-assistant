using System.Data;
using knowledgeBase.Entities;
using knowledgeBase.DataBase;

namespace knowledgeBase.Repositories;

public class SessionRepository : BaseRepository<Session, string>
{
    private readonly IDatabaseConnection _databaseConnection;
    
    public SessionRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }
    
    public async override Task<Session> GetById(string sessionId)
    {
        var sql = @"SELECT SessionId, UserEmail, EndTime FROM ""Session"" WHERE SessionId = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = sessionId
        };
        
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return Mapper.MapToSession(reader);
        }
        
        return null;
    }

    public async override Task<List<Session>> GetAll()
    {
        var sql = @"select * from ""Session""";
        var sessions = new List<Session>();
        
        using var reader = await _databaseConnection.ExecuteReader(sql);
        if (reader.Read())
        {
            sessions.Add(Mapper.MapToSession(reader));
        }
        
        return sessions;
    }

    public async override Task<bool> Create(Session session)
    {
        var createSql = @"insert into ""Session"" (sessionid, useremail, endtime) 
                values (@SessionId, @UserEmail, @EndTime);";
        var createParameters = new Dictionary<string, object>
        {
            ["@SessionId"] = session.SesisonId,
            ["@UserEmail"] = session.UserEmail,
            ["@EndTime"] = session.EndTime
        };

        return await _databaseConnection.ExecuteNonQuery(createSql, createParameters) > 0;
    }

    public async override Task<bool> Update(Session session)
    {
        var sql = @"update Session 
                set ""User"" = @User, EndTime = @EndTime 
                where SessionId = @SessionId";
        var parameters = new Dictionary<string, object>
        {
            ["@SessionId"] = session.SesisonId,
            ["@User"] = session.UserEmail,
            ["@EndTime"] = session.EndTime
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async override Task<bool> Delete(string sessionId)
    {
        var sql = @"delete from ""Session"" where SessionId = @SessionId";
        var parameters = new Dictionary<string, object>
        {
            ["@SessionId"] = sessionId,
        };

        return await _databaseConnection.ExecuteNonQuery(sql, parameters) > 0;
    }

    public async Task<User> GetUserBySessionId(string sessionId)
    {
        var sql = @"select ""User"".email, ""User"".password, ""User"".""name"", ""User"".roleid 
                    from ""Session"" join ""User"" on ""Session"".useremail = ""User"".email
                    where sessionid = @SessionId";
        var parameters = new Dictionary<string, object>
        {
            ["@SessionId"] = sessionId
        };
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return Mapper.MapToUser(reader);
        }
        
        return null;
    }

    public async Task<string> GetRoleBySessionId(string sessionId)
    {
        var sql = @"SELECT Role.Name 
                    FROM ""Session"" JOIN ""User"" ON Sessions.""User"" = ""User"".Email
                    JOIN Role ON Role.RoleId = ""User"".RoleId
                    WHERE SessionId = @SessionId";
        var parameters = new Dictionary<string, object>
        {
            ["@SessionId"] = sessionId
        };
        using var reader = await _databaseConnection.ExecuteReader(sql, parameters);
        if (reader.Read())
        {
            return (string)reader["Role.Name"];
        }
        return null;
    }
    
}