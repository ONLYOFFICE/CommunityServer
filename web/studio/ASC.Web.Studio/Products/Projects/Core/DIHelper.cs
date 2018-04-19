
using System;
using System.Collections.Generic;
using System.Web;
using ASC.Common.Data;
using ASC.Common.DependencyInjection;
using ASC.Common.Web;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Web.Studio.Utility;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using log4net;

namespace ASC.Web.Projects.Core
{
    public static class DIHelper
    {
        internal static IContainer Builder;
        private static bool isRegistered;
        private static readonly object Locker = new object();
        private static readonly ILog Logger = LogManager.GetLogger("ASC");

        public static void Register()
        {
            if (isRegistered) return;

            lock (Locker)
            {
                if (isRegistered) return;

                var container = AutofacConfigLoader.Load("projects");

                container.Register(c => DbManager.FromHttpContext(Global.DbID))
                    .AsSelf()
                    .As<IDbManager>()
                    .InstancePerRequest();

                container.Register(c => new ProjectSecurityCommon())
                    .AsSelf()
                    .InstancePerRequest()
                    .PropertiesAutowired();

                container.Register(c => new ProjectSecurityProject())
                    .AsSelf()
                    .As<ProjectSecurityTemplate<Project>>()
                    .InstancePerRequest()
                    .PropertiesAutowired();

                container.Register(c => new ProjectSecurityTask())
                    .AsSelf()
                    .As<ProjectSecurityTemplate<Task>>()
                    .InstancePerRequest()
                    .PropertiesAutowired();

                container.Register(c => new ProjectSecurityMilestone())
                    .AsSelf()
                    .As<ProjectSecurityTemplate<Milestone>>()
                    .InstancePerRequest()
                    .PropertiesAutowired();

                container.Register(c => new ProjectSecurityMessage())
                    .AsSelf()
                    .As<ProjectSecurityTemplate<Message>>()
                    .InstancePerRequest()
                    .PropertiesAutowired();

                container.Register(c => new ProjectSecurityTimeTracking())
                    .AsSelf()
                    .As<ProjectSecurityTemplate<TimeSpend>>()
                    .InstancePerRequest()
                    .PropertiesAutowired();

                Builder = container.Build();
                isRegistered = true;
            }
        }

        public static ILifetimeScope Resolve(bool disableNotification = false)
        {
            return Resolve(TenantProvider.CurrentTenantID, disableNotification);
        }

        public static ILifetimeScope Resolve(int tenantID, bool disableNotification = false)
        {
            Register();

            var scope = FromHttpContext();
            scope.Resolve<IDbManager>();

            var tenantParameter = new TypedParameter(typeof(int), tenantID);
            var disableNotificationParameter = new TypedParameter(typeof(bool), disableNotification);

            scope.Resolve<IDaoFactory>(tenantParameter);
            scope.Resolve<EngineFactory>(disableNotificationParameter);

            return scope;
        }

        private static ILifetimeScope FromHttpContext()
        {
            if (HttpContext.Current != null)
            {
                var scope = DisposableHttpContext.Current["scope"] as ILifetimeScope;
                if (scope == null)
                {
                    scope = ToHttpContext();
                }
                else
                {
                    scope = new LifeTimeScopeProxy(scope); 
                    try
                    {
                        scope.Resolve<IDbManager>();
                    }
                    catch (ObjectDisposedException e)
                    {
                        Logger.Error("FromHttpContext ObjectDisposedException", e);
                        scope = ToHttpContext();
                    }
                }

                return scope;
            }

            return Builder.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        }

        private static ILifetimeScope ToHttpContext()
        {
            var scope = Builder.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var proxy = new LifeTimeScopeProxy(scope);
            DisposableHttpContext.Current["scope"] = scope;
            return proxy;
        }

        public static TypedParameter GetParameter<T>(T data)
        {
            return new TypedParameter(typeof(T), data);
        }
    }

    internal class LifeTimeScopeProxy : ILifetimeScope
    {
        private ILifetimeScope LifetimeScope { get; set; }

        public LifeTimeScopeProxy(ILifetimeScope lifetimeScope)
        {
            LifetimeScope = lifetimeScope;
            LifetimeScope.ChildLifetimeScopeBeginning += ChildLifetimeScopeBeginning;
            LifetimeScope.CurrentScopeEnding += CurrentScopeEnding;
            LifetimeScope.ResolveOperationBeginning += ResolveOperationBeginning;
        }

        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return LifetimeScope.ResolveComponent(registration, parameters);
        }

        public IComponentRegistry ComponentRegistry { get { return LifetimeScope.ComponentRegistry; } }
        public void Dispose()
        {
            if (HttpContext.Current == null)
            {
                LifetimeScope.Dispose();
            }
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            return LifetimeScope.BeginLifetimeScope();
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return LifetimeScope.BeginLifetimeScope(tag);
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return LifetimeScope.BeginLifetimeScope(configurationAction);
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return LifetimeScope.BeginLifetimeScope(tag, configurationAction);
        }

        public IDisposer Disposer { get { return LifetimeScope.Disposer; } }
        public object Tag { get { return LifetimeScope.Tag; } }

        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;
    }
}