namespace ClubExample.Core.Domain;

public class Member
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; } 
    public required string Name { get; set; }
    public required string Email { get; set; }
    public Guid SubscriptionId { get; set; }
}
