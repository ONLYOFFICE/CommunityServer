using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;

using ASC.Core;
using ASC.Web.Core.Files;

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
            var result = new BuildVersion
                             {
                                 CommunityServer = WebConfigurationManager.AppSettings["version.number"] ?? "8.5.0"
                             };

            try
            {
                using (var client = new WebClient())
                {
                    var data = Encoding.UTF8.GetString(client.DownloadData(FilesLinkUtility.DocServiceApiUrl));
                    var matches = Regex.Matches(data, @"DocsAPI.DocEditor.version = function\(\) \{
        return '(\S+)';
    \};");
                    if (matches.Count > 0 && matches[0].Groups.Count > 1)
                        result.DocumentServer = matches[0].Groups[1].Value;
                }
            }
            catch (Exception)
            {

            }

            try
            {
                var serverDal = new Mail.Server.Dal.ServerDal(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                var requestUrl = serverDal.GetTenantServer().ApiVersionUrl;

                using (var client = new WebClient())
                {
                    var response = Encoding.UTF8.GetString(client.DownloadData(requestUrl));
                    result.MailServer = JObject.Parse(response)["global_vars"]["value"].ToString();
                }
            }
            catch (Exception)
            {

            }

            return result;
        }
    }
}
