using System.Text.Json;
using BCrypt.Net;

public class User
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class UserService
{
    private readonly string _file = Path.Combine(AppContext.BaseDirectory, "data", "users.json");
    private readonly Dictionary<string, User> _users = new();

    public UserService()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
        if (File.Exists(_file))
        {
            var json = File.ReadAllText(_file);
            var list = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            foreach (var u in list)
            {
                _users[u.Email] = u;
            }
        }
    }

    public bool AddUser(string email, string password)
    {
        if (_users.ContainsKey(email)) return false;
        var user = new User { Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(password) };
        _users[email] = user;
        Save();
        return true;
    }

    public bool ValidateCredentials(string email, string password)
    {
        if (_users.TryGetValue(email, out var user))
        {
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        return false;
    }

    private void Save()
    {
        var list = _users.Values.ToList();
        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_file, json);
    }
}
