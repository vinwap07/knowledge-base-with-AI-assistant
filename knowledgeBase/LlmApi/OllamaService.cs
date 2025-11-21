using System.Text;

namespace knowledgeBase;

public class OllamaService
{
    private const string Prompt = @"Нужно написать саммари к тексту ниже. Ничего кроме саммари писать не нужно! 
        Саммари должно уменьшить объем текста вдвое как минимум. 
        Максимальный объем саммари - 3 абзаца.";

    public static async Task<string> SendRequest(string text)
    {
        using var httpClient = new HttpClient();
        
        // Добавляем заголовок авторизации если есть API ключ
        string apiKey = "7720213b2601450c9e5e755f2ac9c074.FsgjMO1mFUzOFYeYwx9Gp917";
        if (!string.IsNullOrEmpty(apiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }

        // JSON данные для запроса
        var jsonData = $@"
        {{
            ""model"": ""gpt-oss:120b"",
            ""prompt"": ""{EscapeJsonString(Prompt + text)}"",
            ""stream"": false
        }}";

        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        
        var response = await httpClient.PostAsync("https://ollama.com/api/generate", content);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }
        
    private static string EscapeJsonString(string value)
    {
        return value.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}