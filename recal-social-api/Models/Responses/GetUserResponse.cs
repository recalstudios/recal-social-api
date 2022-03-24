namespace recal_social_api.Models.Responses;

public class GetUserResponse
{
    public int? Id { get; set;}
    public string? Username { get; set;} = null!;
    public string? Password { get; set;} = null!;
    public string? Email { get; set;} = null!;
    public string? Pfp { get; set;} = null!;
    public string? Status { get; set; }
}