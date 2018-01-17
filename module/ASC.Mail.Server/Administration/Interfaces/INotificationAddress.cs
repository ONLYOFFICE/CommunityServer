
namespace ASC.Mail.Server.Administration.Interfaces
{
    public interface INotificationAddress
    {
        IWebDomain Domain { get; }
        string LocalPart { get; }
        string Email { get; }
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpAccount { get; }
        bool SmtpAuth { get; }
        string SmptEncryptionType { get; set; }
        string SmtpAuthenticationType { get; set; }
    }
}
