namespace knowledgeBase.DataBase;

public class DatabaseInitializer
{
    private readonly IDatabaseConnection _connection;
    
    public DatabaseInitializer(IDatabaseConnection connection)
    {
        _connection = connection;
    }
    
    public async Task InitializeAsync()
    {
        await CreateTablesAsync();
        // await SeedDataAsync();
    }
    
    private async Task CreateTablesAsync()
    {
        string sqlFilePath = "public/createDataBase.sql";

        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException($"SQL файл не найден: {sqlFilePath}");
        }

        try
        {
            using var fileStream = File.Open(sqlFilePath, FileMode.Open);
            using var reader = new StreamReader(fileStream);
        
            string sql = await reader.ReadToEndAsync();
        
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new InvalidOperationException("SQL файл пуст");
            }

            await _connection.ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при выполнении SQL скрипта: {ex.Message}", ex);
        }
    }
    
    private async Task SeedDataAsync()
    {
        var result = await _connection.ExecuteScalar(
            "SELECT COUNT(*) FROM Users");
        bool isInt = int.TryParse((string?)result, out var userCount);
        
        if (userCount == 0)
        {
            var file = File.Open("/addEntities.sql", FileMode.Open);
            var reader = new StreamReader(file);
            string sql = reader.ReadToEnd();
        
            await _connection.ExecuteNonQuery(sql);
        }
    }
}