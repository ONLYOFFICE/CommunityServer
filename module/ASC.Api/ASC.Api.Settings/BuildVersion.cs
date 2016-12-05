using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Configuration;

using ASC.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Services.DocumentService;
using Newtonsoft.Json.Linq;

namespace ASC.Api.Settings
{
    [DataContract(Name = "buildversion", Namespace = "")]
    public class BuildVersion
    {
        [DataMember]
        public string CommunityServer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DocumentServer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MailServer { get; set; }

        public static BuildVersion GetCurrentBuildVersion()
        {
            return new BuildVersion
                {
                    CommunityServer = GetCommunityVersion(),
                    DocumentServer = GetDocumentVersion(),
                    MailServer = GetMailVersion()
                };
        }

        private static string GetCommunityVersion()
        {
            return WebConfigurationManager.AppSettings["version.number"] ?? "8.5.0";
        }

        private static string GetDocumentVersion()
        {
            if (string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl))
                return null;

            return DocumentServiceConnector.GetVersion();
        }

        private static string GetMailVersion()
        {
            String url = null;

            // GetTenantServer() throw System.IO.InvalidDataException if no mail servers registered.
            try
            {
                var serverDal = new Mail.Server.Dal.ServerDal(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                url = serverDal.GetTenantServer().ApiVersionUrl;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof (SettingsApi)).Warn(e.Message, e);
            }

            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                using (var client = new WebClient())
                {
                    var response = Encoding.UTF8.GetString(client.DownloadData(url));
                    return JObject.Parse(response)["global_vars"]["value"].ToString();
                }
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(SettingsApi)).Error(e.Message, e);
            }

            return null;
        }
    }
}
