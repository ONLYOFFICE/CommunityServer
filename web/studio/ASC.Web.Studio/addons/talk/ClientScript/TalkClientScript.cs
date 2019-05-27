/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Data.Storage;
using ASC.ElasticSearch;
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
                        soundsHtml = new List<string>() { 
                            WebPath.GetPath("/addons/talk/swf/startupsound.mp3"),
                            WebPath.GetPath("/addons/talk/swf/incmsgsound.mp3"),
                            WebPath.GetPath("/addons/talk/swf/letupsound.mp3"), 
                            WebPath.GetPath("/addons/talk/swf/sndmsgsound.mp3"),
                            WebPath.GetPath("/addons/talk/swf/statussound.mp3")
                        },
                        fullText = FactoryIndexer<JabberWrapper>.CanSearchByContent()
                    }
                })
            };
        }

        protected override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey + SecurityContext.CurrentAccount.ID + FactoryIndexer<JabberWrapper>.CanSearchByContent() +
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