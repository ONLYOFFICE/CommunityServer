using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Client;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Talk.Addon;
using ASC.Web.Talk.Resources;

namespace ASC.Web.Talk.ClientScript
{
    public class TalkClientScript : Core.Client.HttpHandlers.ClientScript
    {
        protected override string BaseNamespace
        {
            get
            {
                return "ASC.TMTalk";
            }
        }
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var config = new TalkConfiguration();
            return new List<KeyValuePair<string, object>>(2)
            {
                RegisterObject(new
                {
                    Icons = new
                    {
                        addonIcon16 = WebImageSupplier.GetAbsoluteWebPath("talk16.png", TalkAddon.AddonID),
                        addonIcon32 = WebImageSupplier.GetAbsoluteWebPath("talk32.png", TalkAddon.AddonID),
                        addonIcon48 = WebImageSupplier.GetAbsoluteWebPath("talk48.png", TalkAddon.AddonID),
                        addonIcon128 = WebImageSupplier.GetAbsoluteWebPath("talk128.png", TalkAddon.AddonID),
                        iconNewMessage = WebImageSupplier.GetAbsoluteWebPath("icon-new-message.ico", TalkAddon.AddonID)
                    }
                }),
                RegisterObject(new
                {
                    Config = new
                    {
                        validSymbols = config.ValidSymbols ?? "",
                        historyLength = config.HistoryLength ?? "",
                        boshUri = config.BoshUri,
                        jabberAccount = GetJabberAccount(),
                        resourcePriority = config.ResourcePriority,
                        clientInactivity = config.ClientInactivity,
                        addonID = TalkAddon.AddonID,
                        enabledMassend = config.EnabledMassend.ToString().ToLower(),
                        enabledConferences = config.EnabledConferences.ToString().ToLower(),
                        requestTransportType = config.RequestTransportType ?? string.Empty,
                        fileTransportType = config.FileTransportType ?? string.Empty,
                        maxUploadSize = SetupInfo.MaxImageUploadSize,
                        sounds =  WebPath.GetPath("/addons/talk/swf/sounds.swf"),
                        expressInstall =  WebPath.GetPath("/addons/talk/swf/expressinstall.swf")
                    }
                })
            };
        }

        protected override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey + SecurityContext.CurrentAccount.ID +
                   (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal
                        ? (CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                           CoreContext.UserManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture))
                        : string.Empty);
        }

        private string GetJidServerPartOfJid()
        {
            var config = new TalkConfiguration();
            string tenantDomain = CoreContext.TenantManager.GetCurrentTenant().TenantDomain;
            if (config.ReplaceDomain && tenantDomain != null && tenantDomain.EndsWith(config.ReplaceToDomain))
            {
                int place = tenantDomain.LastIndexOf(config.ReplaceToDomain);
                if (place >= 0)
                {
                    return tenantDomain.Remove(place, config.ReplaceToDomain.Length).Insert(place, config.ReplaceFromDomain);
                }
            }
            return tenantDomain;
        }

        private String GetJabberAccount()
        {
            try
            {
                return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName.ToLower() + "@" +
                       GetJidServerPartOfJid() + "/TMTalk";
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }

    public class TalkClientScriptLocalization : Core.Client.HttpHandlers.ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get
            {
                return "ASC.TMTalk";
            }
        }
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(2)
            {
                RegisterResourceSet("Resources", TalkResource.ResourceManager),
                RegisterObject(new
                {
                    statusTitles = new
                    {
                        offline = TalkResource.StatusOffline,
                        online = TalkResource.StatusOnline,
                        away = TalkResource.StatusAway,
                        xa = TalkResource.StatusNA
                    },
                    abbreviatedMonthNames = string.Join(",", CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames),
                    abbreviatedDayNames = string.Join(",", CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames),
                    maxUploadSizeError = FileSizeComment.FileImageSizeExceptionString
                })
            };
        }
    }
}