using ASC.Common.Data;
using ASC.Common.DependencyInjection;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Web.Studio.Utility;
using Autofac;

namespace ASC.Web.CRM.Core
{
    public static class DIHelper
    {
        internal static IContainer Builder;
        private static bool isRegistered;
        private static readonly object Locker = new object();

        public static void Register()
        {
            if (isRegistered) return;

            lock (Locker)
            {
                if (isRegistered) return;

                var container = AutofacConfigLoader.Load("crm");

                container.Register(c => DbManager.FromHttpContext("default"))
                    .AsSelf()
                    .As<IDbManager>()
                    .InstancePerRequest();

                Builder = container.Build();
                isRegistered = true;
            }
        }

        public static ILifetimeScope Resolve()
        {
            return Resolve(TenantProvider.CurrentTenantID);
        }


        public static ILifetimeScope Resolve(int tenantID)
        {
            Register();

            var scope = Builder.BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            scope.Resolve<IDbManager>();
            scope.Resolve<DaoFactory>(new TypedParameter(typeof(int), tenantID));

            return scope;
        }
    }
}