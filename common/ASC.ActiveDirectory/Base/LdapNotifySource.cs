using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASC.ActiveDirectory.Base.Settings;
using ASC.Core.Tenants;
using ASC.Notify.Model;

namespace ASC.ActiveDirectory.Base
{
    class LdapNotifySource : INotifySource
    {
        public Tenant Tenant { get; set; }

        public string ID
        {
            get { return "asc.activedirectory." + Tenant.TenantId; }
        }

        public LdapNotifySource(Tenant tenant)
        {
            Tenant = tenant;
        }

        public void AutoSync(DateTime date)
        {
            LdapNotifyHelper.AutoSync(Tenant);
        }

        public IActionProvider GetActionProvider()
        {
            throw new NotImplementedException();
        }

        public Notify.Patterns.IPatternProvider GetPatternProvider()
        {
            throw new NotImplementedException();
        }

        public Notify.Recipients.IRecipientProvider GetRecipientsProvider()
        {
            throw new NotImplementedException();
        }

        public ISubscriptionProvider GetSubscriptionProvider()
        {
            throw new NotImplementedException();
        }
    }
}
