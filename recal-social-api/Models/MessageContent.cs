namespace recal_social_api.Models;

public class MessageContent
{
        public string Text { get; set; } = null!;
        
        public IEnumerable<MessageAttachement>? Attachments { get; set; }
}