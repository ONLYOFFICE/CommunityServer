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
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Data.Storage;

namespace ASC.Web.CRM.Configuration
{
    [WebZone(WebZoneType.CustomProductList)]
    public class VoipModule : IAddon, IRenderCustomNavigation
    {
        public Guid ID
        {
            get { return WebItemManager.VoipModuleID; }
        }

        public string Name
        {
            get { return CRMVoipResource.VoipModuleTitle; }
        }

        public string Description
        {
            get { return CRMVoipResource.VoipModuleDescription; }
        }

        public string StartURL
        {
            get { return PathProvider.StartURL() + "settings.aspx?type=voip.common&sysname=/modules/voip"; }
        }

        public string HelpURL
        {
            get { return ""; }
        }

        public string ProductClassName { get { return "voip"; } }

        public bool Visible { get { return SetupInfo.VoipEnabled == "true"; } }

        public AddonContext Context { get; private set; }

        public void Init()
        {
            Context = new AddonContext
                      {
                          DefaultSortOrder = 90,
                          IconFileName = "voip_logo.png",
                          CanNotBeDisabled = true
                      };
        }

        public void Shutdown()
        {

        }

        WebItemContext IWebItem.Context
        {
            get { return Context; }
        }

        public Control LoadCustomNavigationControl(Page page)
        {
            return null;
        }

        public string RenderCustomNavigation(Page page)
        {
            try
            {
                if (!VoipNumberData.CanMakeOrReceiveCall) return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            page.RegisterBodyScripts("~/js/asc/core/voip.navigationitem.js");

            return
                string.Format(@"<li class=""top-item-box voip display-none"">
                                  <a class=""voipActiveBox inner-text"" title=""{0}"">
                                      <svg><use base=""{1}"" href=""/skins/default/Images/svg/top-studio-menu.svg#svgTopStudioMenuphone""></use></svg>
                                      <span class=""inner-label"">{2}</span>
                                  </a>
                                </li>",
                              "VoIP",
                              WebPath.GetPath("/"),
                              0);
        }
    }
}