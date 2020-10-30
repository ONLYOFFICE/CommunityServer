
using System.Collections.Generic;

using ASC.Web.Core;

namespace ASC.Web.Files
{
    public partial class Warmup : WarmupPage
    {
        protected override List<string> Exclude
        {
            get
            {
                return new List<string>(3)
                {
                    "DocEditor.aspx",
                    "App.aspx",
                    "Share.aspx"
                };
            }
        }
    }
}