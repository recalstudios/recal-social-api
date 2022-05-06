namespace recal_social_api.Models;

public class MessageAttachement
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public string Type {get;set;} = null!;

    public string Src { get; set; } = null!;
}