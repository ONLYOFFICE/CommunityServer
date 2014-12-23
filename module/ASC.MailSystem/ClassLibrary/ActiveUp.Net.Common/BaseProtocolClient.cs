using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ActiveUp.Net.Mail;


namespace ActiveUp.Net.Common
{
    public abstract class BaseProtocolClient : TcpClient
    {
#if !PocketPC
        protected System.Net.Security.SslStream _sslStream;
#endif
        public virtual string SendEhloHelo()
        {
            var domain = System.Net.Dns.GetHostName();
            return SendEhloHelo(domain);
        }

        public virtual string SendEhloHelo(string domain)
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
            if (this._sslStream != null && this._sslStream.IsAuthenticated) 
                return this._sslStream;
#endif
            return base.GetStream();
        }

        public virtual string Command(string command, int expectedResponseCode)
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

        public virtual string Ehlo(string domain)
        {
            return this.Command("ehlo " + domain, 250);
        }


        public virtual string Helo(string domain)
        {
            return this.Command("helo " + domain, 250);
        }

        public virtual string StartTLS(string host)
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

        protected virtual void DoSslHandShake(ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            ActiveUp.Net.Mail.Logger.AddEntry("DoSslHandShake:Creating SslStream...", 2); 
            this._sslStream = new System.Net.Security.SslStream(base.GetStream(), false, sslHandShake.ServerCertificateValidationCallback, sslHandShake.ClientCertificateSelectionCallback);
            ActiveUp.Net.Mail.Logger.AddEntry("DoSslHandShake:AuthenticateAsClient...", 2);
            try
            {
                this._sslStream.AuthenticateAsClient(sslHandShake.HostName, sslHandShake.ClientCertificates, sslHandShake.SslProtocol, sslHandShake.CheckRevocation);
            }
            catch (Exception ex)
            {
                ActiveUp.Net.Mail.Logger.AddEntry(string.Format("DoSslHandShake:AuthenticateAsClient failed with Exception {0}", ex.ToString()), 2);
                this._sslStream = null;
                throw;
            }
        }

        public abstract string ConnectPlain(string host, int port);
        public abstract IAsyncResult BeginConnectPlain(string host, int port, AsyncCallback callback);

        public abstract string ConnectSsl(string host, int port);
        public abstract IAsyncResult BeginConnectSsl(string host, int port, AsyncCallback callback);

        public abstract string Login(string username, string password);
        public abstract IAsyncResult BeginLogin(string username, string password, AsyncCallback callback);

        public abstract string Authenticate(string username, string password, SaslMechanism mechanism);
        public abstract IAsyncResult BeginAuthenticate(string username, string password, SaslMechanism mechanism, AsyncCallback callback);

        public abstract bool IsConnected { get; }

        public abstract string Disconnect();
        public abstract IAsyncResult BeginDisconnect(AsyncCallback callback);

        public virtual string EndAsyncOperation(IAsyncResult result)
        {
            ActiveUp.Net.Mail.Logger.AddEntry("EndAsyncOperation...", 2);
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }
    }
}
