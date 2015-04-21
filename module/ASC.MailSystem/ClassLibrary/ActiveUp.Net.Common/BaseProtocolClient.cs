using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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

        public bool CertificatePermit { get; set; }

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
            GetStream().Write(Encoding.GetEncoding("iso-8859-1").GetBytes(command + "\r\n"), 0, command.Length + 2);

            using (var sr = new StreamReader(GetStream(), Encoding.GetEncoding("iso-8859-1"), false,
                                                       Client.ReceiveBufferSize, true))
            {
                sr.BaseStream.ReadTimeout = Client.ReceiveTimeout;

                var buffer = new StringBuilder();

                while (true)
                {
                    var line = sr.ReadLine();

                    if (line == null)
                        throw new EndOfStreamException("Unexpected end of stream");

                    if (line.StartsWith(expectedResponseCode + "-"))
                    {
                        buffer.Append(line + "\r\n");
                    }
                    else if (line.StartsWith(expectedResponseCode + " "))
                    {
                        buffer.Append(line + "\r\n");
                        break;
                    }
                    else
                        throw new Exception("Command \"" + (command.Length < 200 ? command : "Data") +
                                            "\" failed : " + line);
                }

                return buffer.ToString();
            }
        }

        public virtual string Ehlo(string domain)
        {
            return this.Command("ehlo " + domain, 250);
        }


        public virtual string Helo(string domain)
        {
            return this.Command("helo " + domain, 250);
        }

        protected virtual void DoSslHandShake(ActiveUp.Net.Security.SslHandShake sslHandShake)
        {
            Logger.AddEntry("DoSslHandShake:Creating SslStream...", 2);
            this._sslStream = new System.Net.Security.SslStream(base.GetStream(), false,
                                                                CertificatePermit
                                                                    ? (sender, certificate, chain, sslPolicyErrors) => true
                                                                    : sslHandShake.ServerCertificateValidationCallback,
                                                                sslHandShake.ClientCertificateSelectionCallback);
            Logger.AddEntry("DoSslHandShake:AuthenticateAsClient...", 2);

            try
            {
                this._sslStream.AuthenticateAsClient(sslHandShake.HostName, sslHandShake.ClientCertificates,
                                                     sslHandShake.SslProtocol, sslHandShake.CheckRevocation);
            }
            catch (Exception ex)
            {
                Logger.AddEntry(string.Format("DoSslHandShake:AuthenticateAsClient failed with Exception {0}", ex), 2);
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

        public abstract string StartTLS(string host);

        public virtual string EndAsyncOperation(IAsyncResult result)
        {
            ActiveUp.Net.Mail.Logger.AddEntry("EndAsyncOperation...", 2);
            return (string)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        public string ConnectSsl(string host, int port, int millisecondsTimeout)
        {
            Logger.AddEntry(
                String.Format("ConnectSsl with timeout = {0} minutes",
                              TimeSpan.FromMilliseconds(millisecondsTimeout).TotalMinutes), 2);

            var func = new Func<string>(() => ConnectSsl(host, port));

            var result = RunMethodWithTimeout(func, millisecondsTimeout);

            return result;
        }

        public string ConnectPlain(string host, int port, int millisecondsTimeout)
        {
            var func = new Func<string>(() => ConnectPlain(host, port));

            var result = RunMethodWithTimeout(func, millisecondsTimeout);

            return result;
        }

        public string Login(string username, string password, int millisecondsTimeout)
        {
            var func = new Func<string>(() => Login(username, password));

            var result = RunMethodWithTimeout(func, millisecondsTimeout);

            return result;
        }

        public string Authenticate(string username, string password, SaslMechanism mechanism, int millisecondsTimeout)
        {
            var func = new Func<string>(() => Authenticate(username, password, mechanism));

            var result = RunMethodWithTimeout(func, millisecondsTimeout);

            return result;
        }

        public struct FuncResult
        {
            public object result;
            public Exception exception;
        }

        public T RunMethodWithTimeout<T>(Func<T> func, int timeout)
        {
            var funcWrap = new Func<FuncResult>(() =>
            {
                var r = new FuncResult { exception = null };
                try
                {
                    r.result = func();
                }
                catch (Exception ex)
                {
                    r.result = null;
                    r.exception = ex;
                }

                return r;
            });

            var result = Execute(funcWrap, timeout);

            if (result.exception != null)
                throw result.exception;

            return (T)result.result;
        }

        public T Execute<T>(Func<T> func, int timeout)
        {
            T result;
            if (!TryExecute(func, timeout, out result))
                throw new TimeoutException();

            return result;
        }

        public bool TryExecute<T>(Func<T> func, int timeout, out T result)
        {
            var t = default(T);
            var thread = new Thread(() => t = func());
            thread.Start();
            var completed = thread.Join(timeout);
            if (!completed)
            {
                Logger.AddEntry(
                            String.Format("TryExecute ended by timeout = {0} minutes. Thread {1} Status = {2}",
                                          TimeSpan.FromMilliseconds(timeout).TotalMinutes, thread.ManagedThreadId, thread.ThreadState), 2);
                thread.Abort();
            }
            result = t;
            return completed;
        }
    }
}
