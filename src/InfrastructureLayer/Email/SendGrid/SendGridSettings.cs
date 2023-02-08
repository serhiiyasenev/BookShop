namespace InfrastructureLayer.Email.SendGrid
{
    public class SendGridSettings
    {
        public string ApiKey { get; set; }
        public string SenderEmailFromKey { get; set; }
        public string SenderNameFrom { get; set; }
    }
}
