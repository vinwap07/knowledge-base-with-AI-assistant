using System.Net;

namespace knowledgeBase;

public static class CookieHelper
{
    public static void SetCookie(HttpListenerResponse response, string value, string cookieName)
    {
        var expires = DateTime.Now.AddMinutes(60).ToString("R");
        var cookieValue = $"{cookieName}={value}; Expires={expires}; Path=/; SameSite=Strict";
        response.Headers.Add("Set-Cookie", cookieValue);
    }

    public static void DeleteCookie(HttpListenerResponse response, string cookieName)
    {
        var cookie = new Cookie(cookieName, "")
        {
            Expires = DateTime.Now.AddDays(-1),
            Path = "/"
        };
        response.Cookies.Add(cookie);
    }

    public static string GetCookieValue(HttpListenerRequest request, string cookieName)
    {
        var cookie = request.Cookies[cookieName];
        if (cookie == null)
        {
            throw new UnauthorizedAccessException("Cookie not found");
        }
        var cookieValue = cookie.Value;
        return cookieValue;
    }
}