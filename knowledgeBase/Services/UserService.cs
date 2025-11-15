using System.Text.Json;
using knowledgeBase.Entities;
using knowledgeBase.Repositories;
using knowledgeBase.View_Models;

namespace knowledgeBase.Services;

public class UserService
{
    private UserRepository _userRepository;
    private SessionRepository _sessionRepository;

    public UserService(UserRepository userRepository, SessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }
    public async Task<List<Article>> GetAllArticlesBySessionId(string sessionId)
    {
        List<Article> articles = new List<Article>();
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        articles = await _userRepository.GetAllFavoriteArticles(user.Email);
        return articles;
    }

    public async Task<string> RegisterNewUser(User user)
    {
        // TODO: валидация данных 
        var isExistst = (await _userRepository.GetById(user.Email)).Name != null;
        if (!isExistst)
        {
            if (await _userRepository.Create(user))
            {
                return "OK";
            }
        }
        return "Email already exists";
    }

    public async Task<UserProfile> UpdateUser(User user)
    { 
        var isUpdated = await _userRepository.Update(user);
        if (!isUpdated)
        {
            throw new ArgumentException("Invalid request body");
        }
        
        var role = await _userRepository.GetRoleById(user.RoleId);
        var userProfile = new UserProfile() { Email = user.Email, Name = user.Name, Role = role};
        return userProfile;
    }

    public async Task<bool> DeleteUserProfile(string sessionId)
    {
        await _sessionRepository.Delete(sessionId);
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return await _userRepository.Delete(user.Email);
    }
    
    public async Task<bool> LogoutUser(string sessionId)
    {
        return await _sessionRepository.Delete(sessionId);
    }

    public async Task<UserProfile> Authenticate(string email, string password)
    {
        // TODO: добавить валидацию email
        // TODO: добавить шифрование пароля
        var user = await _userRepository.GetById(email);
        var role = await _userRepository.GetRoleById(user.RoleId);

        if (user.Email != null && user.Email != string.Empty)
        {
            if (user.Password == password)
            {
                return new UserProfile() {Email = email, Name = user.Name, Role = role};
            }
            
            return new UserProfile();
        }
        
        return new UserProfile();
    }

    public async Task<string> GetRoleBySessionId(string sessionId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        
        var role = await _userRepository.GetRoleById(user.RoleId);
        return role;
    }

    public async Task<UserProfile> GetUserProfileBySessionId(string id)
    {
        var user = await _sessionRepository.GetUserBySessionId(id);
        var userProfile = new UserProfile() { Email = user.Email, Name = user.Name };
        
        var role = await _userRepository.GetRoleById(user.RoleId);
        userProfile.Role = role;
        
        return userProfile;
    }

    public async Task<string> CreateSession(string clientEmail)
    {
        var sessionId = Guid.NewGuid().ToString() + "-" + DateTime.Now.Ticks;
        var endTime = DateTime.UtcNow.AddHours(1);
        
        _sessionRepository.Create(new Session() {SesisonId = sessionId, UserEmail = clientEmail, EndTime = endTime});
        return sessionId;
    }

    public async Task<bool> AddArticleToFavorite(string sessionId, int articleId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return await _userRepository.AddArticleToFavorite(user.Email, articleId);
    }

    public async Task<bool> RemoveArticleFromFavorite(string sessionId, int articleId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return await _userRepository.RemoveArticleFromFavorite(user.Email, articleId);
    }
}