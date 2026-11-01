namespace InvoiceApp.Api.Auth;

public record UserSession(string Username, string Role);

public class DemoUser
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class DemoUsersOptions
{
    public List<DemoUser> Users { get; set; } = new();
}
