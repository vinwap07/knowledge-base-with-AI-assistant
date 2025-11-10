/*
using System.Net;
using knowledgeBase.Services;

namespace knowledgeBase.Controllers;

public class ArticleController : BaseController
{
    public HttpListenerContext Context { get; set; }
    
    private ArticleService _articleService;
    public async override Task<string> HandleRequest()
    {
        switch (Context.Request.HttpMethod)
        {
            case "GET":
                if (Context.Request.Url.LocalPath == "/article/favorite")
                {
                    
                }
                else if (Context.Request.Url.LocalPath == "/article/all")
                {
                    
                }
                else
                {
                    
                }
                break;
            case "POST":
                \
                break;
            case "DELETE":
                
                break;
            default:
                
        }
    }
}
*/