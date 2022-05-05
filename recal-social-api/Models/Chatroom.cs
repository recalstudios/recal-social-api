namespace recal_social_api.Models;

public class Chatroom
{
    public int ChatroomId { get; set; }
    public string Name { get; set; } = null!;
    public string Image { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Pass { get; set; } = null!;
    public DateTime LastActive { get; set; }
}