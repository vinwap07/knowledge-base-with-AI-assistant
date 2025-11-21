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

    public async Task<string> RegisterNewUser(User user)
    {
        // TODO: валидация данных 
        user.RoleId = 1;
        user.Password = PasswordHasher.Hash(user.Password);
        
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

    public async Task<bool> UpdateUser(User user)
    { 
        var isUpdated = await _userRepository.Update(user);
        return isUpdated;
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

    public async Task<User> Authenticate(User incomingUser)
    {
        // TODO: добавить валидацию email
        var user = await _userRepository.GetById(incomingUser.Email);
        var role = await _userRepository.GetRoleByRoleId(user.RoleId);
        if (user.Email != null && user.Email != string.Empty)
        {
            if (PasswordHasher.Validate(user.Password, incomingUser.Password))
            {
                return user;
            }
            
            return new User();
        }
        
        return new User();
    }

    public async Task<string> GetUserRole(User user)
    {
        var role = await _userRepository.GetRoleByEmail(user.Email);
        return role;
    }

    public async Task<string> GetRoleBySessionId(string sessionId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        if (user == null)
        {
            return "unknown";
        }
        
        var role = await _userRepository.GetRoleByRoleId(user.RoleId);
        return role;
    }

    public async Task<User> GetUserBySessionId(string sessionId)
    {
        var user = await _sessionRepository.GetUserBySessionId(sessionId);
        return user;
    }

    public async Task<string> CreateSession(User user)
    {
        var sessionId = Guid.NewGuid().ToString() + "-" + DateTime.Now.Ticks;
        var endTime = DateTime.UtcNow.AddHours(1);
        
        _sessionRepository.Create(new Session() {SesisonId = sessionId, UserEmail = user.Email, EndTime = endTime});
        return sessionId;
    }
}