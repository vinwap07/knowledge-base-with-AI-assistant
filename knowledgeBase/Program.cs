using System.Net;
using knowledgeBase;
using knowledgeBase.Middleware;
using knowledgeBase.DataBase;
using knowledgeBase.Controllers;
using knowledgeBase.Services;
using knowledgeBase.Repositories;

var connectionString = "Host=localhost;Username=postgres;Password=vinwap07;Database=knowledge_base";
PostgresDbConnection dbConnection = new PostgresDbConnection(connectionString);
DatabaseInitializer dbInitializer = new DatabaseInitializer(dbConnection);
await dbInitializer.InitializeAsync();

var userRepository = new UserRepository(dbConnection);
var sessionRepository = new SessionRepository(dbConnection);
var userService = new UserService(userRepository, sessionRepository);
var articleRepository = new ArticleRepository(dbConnection);
var articleService = new ArticleService(articleRepository, sessionRepository);
var articleController = new ArticleController(articleService, userService);
var userController = new UserController(userService, articleService);


RouteTable routeTable = new RouteTable();
routeTable.Get("/user/getProfile", 
    async (context, parameters) => await userController.GetUserProfile(context, parameters));
routeTable.Post("/user/login", 
    async (context, parameters) => await userController.LoginUser(context, parameters), true);
routeTable.Post("/user/register",
    async (context, parameters) => await userController.RegisterNewUser(context, parameters), true);
routeTable.Post("/user/logout", 
    async (context, parameters) => await userController.LogoutUser(context, parameters));
routeTable.Post("/user/update", 
    async (context, parameters) => await userController.UpdateUserProfile(context, parameters));
routeTable.Post("/user/toFavorite", 
    async (context, parameters) => await userController.AddArticleToFavorite(context, parameters));
routeTable.Delete("/user/toUnFavorite", 
    async (context, parameters) => await userController.RemoveArticleFromFavorite(context, parameters));
routeTable.Delete("user/profile", 
    async (context, parameters) => await userController.DeleteUserProfile(context, parameters));
routeTable.Get("/user/favorite", 
    async (context, parameters) => await userController.GetFavoriteArticlesPreview(context, parameters));
routeTable.Post("/article/create",
    async (context, parameters) => await articleController.CreateArticle(context, parameters));
routeTable.Get("article/{articleId}", 
    async (context, parameters) => await articleController.GetArticlePage(context, parameters));
routeTable.Get("article/byAuthor/{author}",
    async (context, parameters) => await articleController.GetArticlePreviewByAuthor(context, parameters));


// TODO: добавить роуты для остальных контроллеров

var middlewarePipeline = new MiddlewarePipeline();
//middlewarePipeline.Use(new LoggingMiddleware());
middlewarePipeline.Use(new ErrorHandlingMiddleware());
middlewarePipeline.Use(new AuthenticationMiddleware(userService, sessionRepository));
middlewarePipeline.Use(new RoutingMiddleware(routeTable));
middlewarePipeline.Use(new StaticFilesMiddleware("public/"));

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:5000/");
listener.Start();

while (listener.IsListening)
{
    var context = await listener.GetContextAsync();
    var myContext = new HttpContext(context, "unknown");
    _ = Task.Run(async () =>
    {
        await middlewarePipeline.ExecuteAsync(myContext);
    });
}