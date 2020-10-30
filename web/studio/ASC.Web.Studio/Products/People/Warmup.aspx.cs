
using System.Collections.Generic;

using ASC.Web.Core;

namespace ASC.Web.People
{
    public partial class Warmup : WarmupPage
    {
        protected override List<string> Exclude
        {
            get
            {
                return new List<string>(1)
                {
                    "Reassigns.aspx"
                };
            }
        }
    }
}