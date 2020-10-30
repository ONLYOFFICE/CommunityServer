/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Configuration;
using System.Net.Sockets;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Logging;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading;
using ASC.Core;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.Settings.Smtp
{
    public class SmtpOperation
    {
        public const string OWNER = "SMTPOwner";
        public const string SOURCE = "SMTPSource";
        public const string PROGRESS = "SMTPProgress";
        public const string RESULT = "SMTPResult";
        public const string ERROR = "SMTPError";
        public const string FINISHED = "SMTPFinished";

        protected DistributedTask TaskInfo { get; set; }

        protected CancellationToken CancellationToken { get; private set; }

        protected int Progress { get; private set; }

        protected string Source { get; private set; }

        protected string Status { get; set; }

        protected string Error { get; set; }

        protected int CurrentTenant { get; private set; }

        protected Guid CurrentUser { get; private set; }

        protected ILog Logger { get; private set; }

        public SmtpSettingsWrapper SmtpSettings { get; private set; }

        private readonly string messageSubject;

        private readonly string messageBody;

        public SmtpOperation(SmtpSettingsWrapper smtpSettings, int tenant, Guid user)
        {
            SmtpSettings = smtpSettings;
            CurrentTenant = tenant;
            CurrentUser = user;

            messageSubject = Web.Studio.Core.Notify.WebstudioNotifyPatternResource.subject_smtp_test;
            messageBody = Web.Studio.Core.Notify.WebstudioNotifyPatternResource.pattern_smtp_test;

            Source = "";
            Progress = 0;
            Status = "";
            Error = "";
            Source = "";

            TaskInfo = new DistributedTask();

            Logger = LogManager.GetLogger("ASC");
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            try
            {
                CancellationToken = cancellationToken;

                SetProgress(5, "Setup tenant");

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SetProgress(10, "Setup user");

                SecurityContext.AuthenticateMe(CurrentUser); //Core.Configuration.Constants.CoreSystem);

                SetProgress(15, "Find user data");

                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                SetProgress(20, "Create mime message");

                var toAddress = new MailboxAddress(currentUser.UserName, currentUser.Email);

                var fromAddress = new MailboxAddress(SmtpSettings.SenderDisplayName, SmtpSettings.SenderAddress);

                var mimeMessage = new MimeMessage
                {
                    Subject = messageSubject
                };

                mimeMessage.From.Add(fromAddress);

                mimeMessage.To.Add(toAddress);

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = messageBody
                };

                mimeMessage.Body = bodyBuilder.ToMessageBody();

                mimeMessage.Headers.Add("Auto-Submitted", "auto-generated");

                using (var client = GetSmtpClient())
                {
                    SetProgress(40, "Connect to host");

                    client.Connect(SmtpSettings.Host, SmtpSettings.Port.GetValueOrDefault(25),
                        SmtpSettings.EnableSSL ? SecureSocketOptions.Auto : SecureSocketOptions.None, cancellationToken);

                    if (SmtpSettings.EnableAuth)
                    {
                        SetProgress(60, "Authenticate");

                        client.Authenticate(SmtpSettings.CredentialsUserName,
                            SmtpSettings.CredentialsUserPassword, cancellationToken);
                    }

                    SetProgress(80, "Send test message");

                    client.Send(FormatOptions.Default, mimeMessage, cancellationToken);
                }

            }
            catch (AuthorizingException authError)
            {
                Error = Resources.Resource.ErrorAccessDenied; // "No permissions to perform this action";
                Logger.Error(Error, new SecurityException(Error, authError));
            }
            catch (AggregateException ae)
            {
                ae.Flatten().Handle(e => e is TaskCanceledException || e is OperationCanceledException);
            }
            catch (SocketException ex)
            {
                Error = ex.Message; //TODO: Add translates of ordinary cases
                Logger.Error(ex.ToString());
            }
            catch (AuthenticationException ex)
            {
                Error = ex.Message; //TODO: Add translates of ordinary cases
                Logger.Error(ex.ToString());
            }
            catch (Exception ex)
            {
                Error = ex.Message; //TODO: Add translates of ordinary cases
                Logger.Error(ex.ToString());
            }
            finally
            {
                try
                {
                    TaskInfo.SetProperty(FINISHED, true);
                    PublishTaskInfo();

                    SecurityContext.Logout();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("LdapOperation finalization problem. {0}", ex);
                }
            }
        }

        public SmtpClient GetSmtpClient()
        {
            var sslCertificatePermit = ConfigurationManagerExtension.AppSettings["mail.certificate-permit"] != null &&
                    Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["mail.certificate-permit"]);

            return new SmtpClient
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                    sslCertificatePermit ||
                    MailKit.MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                Timeout = (int) TimeSpan.FromSeconds(30).TotalMilliseconds
            };
        }

        public virtual DistributedTask GetDistributedTask()
        {
            FillDistributedTask();
            return TaskInfo;
        }

        protected virtual void FillDistributedTask()
        {
            TaskInfo.SetProperty(SOURCE, Source);
            TaskInfo.SetProperty(OWNER, CurrentTenant);
            TaskInfo.SetProperty(PROGRESS, Progress < 100 ? Progress : 100);
            TaskInfo.SetProperty(RESULT, Status);
            TaskInfo.SetProperty(ERROR, Error);
            //TaskInfo.SetProperty(PROCESSED, successProcessed);
        }

        protected int GetProgress()
        {
            return Progress;
        }

        const string PROGRESS_STRING = "Progress: {0}% {1} {2}";

        public void SetProgress(int? currentPercent = null, string currentStatus = null, string currentSource = null)
        {
            if (!currentPercent.HasValue && currentStatus == null && currentSource == null)
                return;

            if (currentPercent.HasValue)
                Progress = currentPercent.Value;

            if (currentStatus != null)
                Status = currentStatus;

            if (currentSource != null)
                Source = currentSource;

            Logger.InfoFormat(PROGRESS_STRING, Progress, Status, Source);

            PublishTaskInfo();
        }

        protected void PublishTaskInfo()
        {
            FillDistributedTask();
            TaskInfo.PublishChanges();
        }
    }
}
