using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using ActiveUp.Net.Mail;


namespace ActiveUp.Net.Common
{
    public abstract class BaseProtocolClient : TcpClient
    {
#if !PocketPC
        protected System.Net.Security.SslStream _sslStream;
#endif
        virtual public string SendEhloHelo()
        {
            var domain = System.Net.Dns.GetHostName();
            return SendEhloHelo(domain);
        }

        virtual public string SendEhloHelo(string domain)
        {
            try
            {
                var hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                var ip = (
                           from addr in hostEntry.AddressList
                           where addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                           select addr.ToString()
                    ).FirstOrDefault();

                if (!string.IsNullOrEmpty(ip)) domain = "[" + ip + "]";
            }
            catch
            {
            }

            try
            {
                return this.Ehlo(domain);
            }
            catch
            {
                return this.Helo(domain);
            }
        }

        public new System.IO.Stream GetStream()
        {
#if !PocketPC
            if (this._sslStream != null) return this._sslStream;
#endif
            return base.GetStream();
        }

        virtual public string Command(string command, int expectedResponseCode)
        {
            //this.OnTcpWriting(new ActiveUp.Net.Mail.TcpWritingEventArgs(command));

            this.GetStream().Write(System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(command + "\r\n"), 0, command.Length + 2);

            System.IO.StreamReader sr = new System.IO.StreamReader(this.GetStream(), System.Text.Encoding.GetEncoding("iso-8859-1"));

            System.Text.StringBuilder buffer = new System.Text.StringBuilder();

            //this.OnTcpWritten(new ActiveUp.Net.Mail.TcpWrittenEventArgs(command));

            string line = "";
            //if(command.Length<200) this.OnTcpReading();
            while (true)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    if (line.StartsWith(expectedResponseCode.ToString() + "-")) buffer.Append(string.Concat(line, Tokenizer.NewLine));
                    else if (line.StartsWith(expectedResponseCode.ToString() + " "))
                    {
                        buffer.Append(string.Concat(line, Tokenizer.NewLine));
                        break;
                    }
                    else throw new Exception("Command \"" + (command.Length < 200 ? command : "Data") + "\" failed : " + line);
                }
                else throw new Exception("Could not read response from the server. The connection has been closed by the remote server.");
            }

            //if(buffer.Length<200) this.OnTcpRead(new ActiveUp.Net.Mail.TcpReadEventArgs(buffer.ToString().Trim(new char[] {'\r','\n'})));

            return buffer.ToString();
        }

        virtual public string Ehlo(string domain)
        {
            return this.Command("ehlo " + domain, 250);
        }


        virtual public string Helo(string domain)
        {
            return this.Command("helo " + domain, 250);
        }


        virtual public string StartTLS(string host)
        {
            try
            {
                var response = this.Command("STARTTLS", 220);

                if (response != "Not supported")
                {
                    this.DoSslHandShake(new ActiveUp.Net.Security.SslHandShake(host));

                    response = this.SendEhloHelo();
                }

                return response;
            }
            catch
            {
                return "Not supported";
            }
        }

        virtual protected void DoSslHandShake(ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            this._sslStream = new System.Net.Security.SslStream(base.GetStream(), false, sslHandShake.ServerCertificateValidationCallback, sslHandShake.ClientCertificateSelectionCallback);
            this._sslStream.AuthenticateAsClient(sslHandShake.HostName, sslHandShake.ClientCertificates, sslHandShake.SslProtocol, sslHandShake.CheckRevocation);
        }

        public abstract IAsyncResult BeginConnect(string host, int port, AsyncCallback callback);
        public abstract IAsyncResult BeginConnectSsl(string host, int port, AsyncCallback callback);
        public abstract string Login(string username, string password, string host);
        public abstract IAsyncResult BeginAuthenticate(string username, string password, SaslMechanism mechanism, AsyncCallback callback);
        public abstract string EndConnectSsl(IAsyncResult result);
        public abstract bool IsConnected { get; }
        public abstract string Disconnect();
    }
}
