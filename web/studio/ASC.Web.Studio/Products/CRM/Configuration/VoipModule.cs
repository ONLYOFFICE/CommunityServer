using System;
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;

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
                                      <span class=""inner-label"">{1}</span>
                                  </a>
                                </li>",
                              "VoIP",
                              0);
        }
    }
}