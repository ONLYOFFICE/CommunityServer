using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Tests
{
    [TestFixture(Description = "SMTP protocol related tests")]
    public class SmtpTests
    {
        [Test(Description = "This test is used to verify that subject encoding is done and later decoded correctly by GMAIL.")]
        public void GmailSubjectEncodingTest()
        {
            Message message = new Message();
            message.From = new Address("user@example.org", "John Doe");
            message.To.Add("[youraccounthere]@gmail.com", "Jean Dupont");
            message.Subject = Codec.RFC2047Encode("Je suis Liégeois et je suis prêt à rencontrer Asger Jørnow", "iso-8859-1");

            message.BodyHtml.Text = "This is some html <b>content</b>";
            message.BodyText.Text = "This is some plain/text content";

            SmtpClient.SendSsl(message, "smtp.gmail.com", 465, "[putyourloginhere]", "[putyourpasshere]", SaslMechanism.Login);
        }
    }
}
