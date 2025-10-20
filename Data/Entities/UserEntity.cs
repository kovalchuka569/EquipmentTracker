using Common.Enums;

namespace Data.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime AccessRequestedAt { get; set; } = DateTime.UtcNow;
    public UserStatus Status { get; set; }
}