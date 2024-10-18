namespace recal_social_api.Models.Responses;

public class UserHasRoomResponse
{
    public int Id { get; set;}

    public string? Username { get; set;}

    public string? Pfp { get; set;}

    public int ChatroomId { get; set; }
}
