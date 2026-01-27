using Microsoft.AspNetCore.Identity;
using TroLiKOC.SharedKernel.Domain;

namespace TroLiKOC.Modules.Identity.Domain.Entities;

public class User : IdentityUser<Guid>, IAggregateRoot
{
    public string DisplayName { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public User(string userName, string email, string displayName) : base(userName)
    {
        Email = email;
        DisplayName = displayName;
    }

    // Required for EF Core
    private User() { }
}
