using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActiveUp.Net.Mail;

namespace ActiveUp.Console.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Start work");

            string sHostPop = "pop.googlemail.com";
            string sHostImap = "imap.googlemail.com";
            int nPort = 995;
            string sUserName = "bart.simpson.4test@gmail.com";
            string sPasword = "theSimpsons";

            try
            {
                string sEml = @"E:\ut8_encripted_teamlab.eml";
                
                ActiveUp.Net.Mail.Message m = 
                ActiveUp.Net.Mail.Parser.ParseMessageFromFile(sEml);
                var header = ActiveUp.Net.Mail.Parser.ParseHeader(sEml);
                
                Pop3Client pop = new Pop3Client();

                // Connect to the pop3 client
                pop.ConnectSsl(sHostPop, nPort, "recent:" + sUserName, sPasword);

                if (pop.MessageCount > 0)
                {
                    ActiveUp.Net.Mail.Message message = pop.RetrieveMessageObject(4);
                    string sHtml = message.BodyHtml.Text;
                }
                else
                    System.Console.WriteLine("No letters!");

                pop.Disconnect();

                Imap4Client imap = new Imap4Client();

                imap.ConnectSsl(sHostImap, 993);

                imap.Login(sUserName, sPasword, "");

                Mailbox inbox = imap.SelectMailbox("inbox");
                
                if (inbox.MessageCount > 0)
                {
                    ActiveUp.Net.Mail.Message message = inbox.Fetch.MessageObject(6);
                    string sHtml = message.BodyHtml.Text;
                }

                imap.Disconnect();

            }
            catch (Exception ex)
            {
                System.Console.Write("\r\n" + ex);
            }

            System.Console.WriteLine("Stop work");

            System.Console.ReadKey();
        }
    }
}
