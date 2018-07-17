using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.People.Masters.ClientScripts
{
    public class ClientSettingsResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.People.Data"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(1)
            {
                RegisterObject(new
                {
                    emptyScreenPeopleFilter = WebImageSupplier.GetAbsoluteWebPath("empty_screen_filter.png")
                })
            };
        }
    }
}