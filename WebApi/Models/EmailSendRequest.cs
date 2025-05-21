namespace WebApi.Models;

public class EmailSendRequest
{
    public List<string> Recipients { get; set; } = new List<string>();
    public string Subject { get; set; } = null!;
    public string PlainText { get; set; } = null!;
    public string Html { get; set; } = null!;
}
