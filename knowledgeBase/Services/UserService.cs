using knowledgeBase.Entities;
using knowledgeBase.Repositories;
using knowledgeBase.View_Models;

namespace knowledgeBase.Services;

public class UserService
{
    private UserRepository _userRepository;
    private RoleRepository _roleRepository;

    public async Task<bool> RegisterNewEmployee(User user)
    {
        // TODO: валидация данных 
        return await _userRepository.Create(user);
    }

    public async Task<User> Authenticate(string email, string password)
    {
        var user = await _userRepository.GetById(email);

        if (user != null)
        {
            if (user.Password == password)
            {
                return user;
            }
            
            return null;
        }
        
        return null;
    }

    public async Task<UserProfile> GetUserProfile(string email)
    {
        var user = await _userRepository.GetById(email);
        var userProfile = new UserProfile() { Email = user.Email, Name = user.Name};
        
        var role = await _roleRepository.GetById(user.RoleId);
        userProfile.Role = role.Name;
        
        return userProfile;
    }
}