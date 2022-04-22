using recal_social_api.Models.Responses;

namespace recal_social_api.Models;

public class Message
{
    public int MessageId { get; set; }
    public string Data { get; set; } = null!;
    public int AuthorId { get; set; }
    public DateTime Time { get; set; }
}