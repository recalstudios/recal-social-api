namespace recal_social_api.Models.Responses;

public class GetUserChatroomsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Image { get; set; } = null!;
    public IEnumerable<UserHasRoomResponse> Users { get; set; } = null!;
    //public string Code { get; set; } = null!;
}