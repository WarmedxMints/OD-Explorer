namespace ODExplorer.Models
{
    public sealed class MailNotification
    {
        public required string Title { get; set; }
        public required string Sender { get; set; }
        public required string Content { get; set; }
    }
}
