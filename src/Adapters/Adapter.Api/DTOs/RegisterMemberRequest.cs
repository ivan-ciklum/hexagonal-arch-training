using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClubExample.Adapter.Api.DTOs;

/// <summary>
/// Request to register a new member in a club
/// </summary>
public sealed record RegisterMemberRequest
{
    /// <summary>
    /// The unique identifier of the club
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public required Guid ClubId { get; init; }
    
    /// <summary>
    /// The member's full name
    /// </summary>
    /// <example>Juan Pérez</example>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; init; }
    
    /// <summary>
    /// The member's email address
    /// </summary>
    /// <example>juan.perez@example.com</example>
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public required string Email { get; init; }
    
    /// <summary>
    /// Type of subscription (Monthly, Yearly, etc.)
    /// </summary>
    /// <example>Monthly</example>
    [Required]
    [StringLength(50)]
    public required string SubscriptionType { get; init; }
}
