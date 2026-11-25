namespace Claims.Api.Domain;

/// <summary>A claimant or a claims officer. <see cref="TeamId"/> groups officers for the workload view.</summary>
public sealed class User
{
    private User()
    {
    }

    public User(Guid id, string name, UserRole role, string? teamId = null)
    {
        Id = id;
        Name = name;
        Role = role;
        TeamId = teamId;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public string? TeamId { get; private set; }
}
