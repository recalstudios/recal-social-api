using recal_social_api.Models.Responses;

namespace recal_social_api.Models;

public class Message
{
    public int Id { get; set; }
    public string Type { get; set; } = null!;
    public int Room { get; set; }
    public int Author { get; set; }

    public MessageContent Content { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}