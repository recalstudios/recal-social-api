using recal_social_api.Models.Responses;

namespace recal_social_api.Models;

public class Message
{
    public int id { get; set; }
    public string type { get; set; } = null!;
    public int room { get; set; }
    public int author { get; set; }

    public new MessageContent content { get; set; } = null!;
    public DateTime timestamp { get; set; }
}