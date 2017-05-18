using ASC.Web.CRM.Controls.Settings;

namespace ASC.Web.CRM
{
    public partial class Calls : BasePage
    {
        protected override void PageLoad()
        {
            CommonContainerHolder.Controls.Add(LoadControl(VoipCalls.Location));
        }
    }
}