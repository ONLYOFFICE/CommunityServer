using System.Collections.Generic;

using ASC.Web.Core;

namespace ASC.Web.Projects
{
    public partial class Warmup : WarmupPage
    {
        protected override List<string> Pages
        {
            get
            {
                return new List<string>(1)
                {
                    "Reports.aspx?reportType=0",
                };
            }
        }

        protected override List<string> Exclude
        {
            get
            {
                return new List<string>(1)
                {
                    "Timer.aspx"
                };
            }
        }
    }
}