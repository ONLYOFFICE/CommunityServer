using System;
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio
{
    public partial class Warmup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WarmUpController.Instance.Execute();
        }
    }
}