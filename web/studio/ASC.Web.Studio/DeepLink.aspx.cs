using System;

using ASC.Web.Studio.UserControls;

namespace ASC.Web.Studio
{
    public partial class DeepLink : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            Master.DisabledSidePanel = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;
            Master.TopStudioPanel.DisableLoginPersonal = true;
            Master.TopStudioPanel.DisableGift = true;

            var deepLinkingControl = (DeepLinking)LoadControl(DeepLinking.Location);
            DeepLinkingHolder.Controls.Add(deepLinkingControl);
        }
    }

}