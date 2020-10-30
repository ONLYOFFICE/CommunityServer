using System.Collections.Generic;

using ASC.Web.Core;

namespace ASC.Web.Studio
{
    public partial class Warmup : WarmupPage
    {
        protected override List<string> Pages
        {
            get
            {
                return new List<string>(10)
                {
                    "Management.aspx?type=1",
                    "Management.aspx?type=2",
                    "Management.aspx?type=3",
                    "Management.aspx?type=4",
                    "Management.aspx?type=5",
                    "Management.aspx?type=6",
                    "Management.aspx?type=7",
                    "Management.aspx?type=10",
                    "Management.aspx?type=11",
                    "Management.aspx?type=15",
                };
            }
        }

        protected override List<string> Exclude
        {
            get
            {
                return new List<string>(5)
                {
                    "Auth.aspx",
                    "403.aspx",
                    "404.aspx",
                    "500.aspx",
                    "PaymentRequired.aspx",
                    "ServerError.aspx",
                    "Tariffs.aspx",
                    "Terms.aspx",
                    "Wizard.aspx"
                };
            }
        }
    }
}