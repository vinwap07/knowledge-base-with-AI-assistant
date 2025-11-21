using System.Net;
using System.Text.Json;
using knowledgeBase;
using knowledgeBase.Middleware;
using knowledgeBase.DataBase;
using knowledgeBase.Controllers;
using knowledgeBase.Services;
using knowledgeBase.Repositories;

var configPath = "public/config.json";
string json = File.ReadAllText(configPath);
Config config = JsonSerializer.Deserialize<Config>(json);
PostgresDbConnection dbConnection = new PostgresDbConnection(config.DatabaseConnectionString);
DatabaseInitializer dbInitializer = new DatabaseInitializer(dbConnection);
await dbInitializer.InitializeAsync();

var userRepository = new UserRepository(dbConnection);
var sessionRepository = new SessionRepository(dbConnection);
var categoryRepository = new CategoryRepository(dbConnection);
var categoryService = new CategoryService(categoryRepository);
var userService = new UserService(userRepository, sessionRepository);
var articleRepository = new ArticleRepository(dbConnection);
var articleService = new ArticleService(articleRepository, sessionRepository, userRepository);
var articleController = new ArticleController(articleService, userService);
var userController = new UserController(userService, articleService);
var categoryController = new CategoryController(categoryService);

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
routeTable.Delete("user/profile", 
    async (context, parameters) => await userController.DeleteUserProfile(context, parameters));

routeTable.Get("/categories",
    async (context, parameters) => await categoryController.GetAllCategories(context, parameters));
routeTable.Get("/categories/slugs",
    async (context, parameters) => await categoryController.GetAllCategoryDTO(context, parameters));

routeTable.Get("/article",
    async (context, parameters) => await articleController.GetAllArticleDTO(context, parameters));
routeTable.Get("/article/favorite", 
    async (context, parameters) => await articleController.GetFavoriteArticlesPreview(context, parameters));
routeTable.Post("/article/create",
    async (context, parameters) => await articleController.CreateArticle(context, parameters));
routeTable.Get("/article/myArticles",
    async (context, parameters) => await articleController.GetMyArticlePreview(context, parameters));
routeTable.Get("/article/checkLike/{articleId}",
    async (context, parameters) => await articleController.CheckLike(context, parameters));
routeTable.Post("/article/like/{articleId}", 
    async (context, parameters) => await articleController.LikeArticle(context, parameters));
routeTable.Delete("/article/like/{articleId}", 
    async (context, parameters) => await articleController.RemoveArticleFromLiked(context, parameters));
routeTable.Get("/article/{articleId}", 
    async (context, parameters) => await articleController.GetArticlePage(context, parameters));
routeTable.Get("/article/popular/{count}",
    async (context, parameters) => await articleController.GetPopularArticlesPreview(context, parameters), true);
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

