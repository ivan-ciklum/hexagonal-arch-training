namespace ClubExample.Core.Domain;

public class Subscription
{
    public Guid Id { get; set; }
    public required string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string Status { get; set; } = string.Empty;
}
