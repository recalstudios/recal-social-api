namespace recal_social_api.Models.Responses;

public class UserHasRoomResponse
{
    public int Id { get; set;}
    
    public string? Username { get; set;} = null!;
    
    public string? Pfp { get; set;} = null!;
    
    public int ChatroomId { get; set; }
}