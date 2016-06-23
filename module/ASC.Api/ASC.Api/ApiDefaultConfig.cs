/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using ASC.Api.Impl;
using ASC.Api.Impl.Invokers;
using ASC.Api.Impl.Responders;
using ASC.Api.Impl.Routing;
using ASC.Api.Impl.Serializers;
using ASC.Api.Interfaces;
using ASC.Api.Interfaces.Storage;
using ASC.Api.Utils;
using Microsoft.Practices.Unity;

namespace ASC.Api
{
    public static class ApiDefaultConfig
    {
        private static void RegisterIfNever<T, TT>(this IUnityContainer container)
        {
            RegisterIfNever<T, TT>(container,(string)null);
        }

        private static void RegisterIfNever<T, TT>(this IUnityContainer container, string name)
        {
            RegisterIfNever<T, TT>(container, name, new SingletonLifetimeManager());
        }

        private static void RegisterIfNever<T, TT>(this IUnityContainer container, LifetimeManager manager)
        {
            RegisterIfNever<T, TT>(container, null, manager);
        }

        private static void RegisterIfNever<T, TT>(this IUnityContainer container, string name, LifetimeManager manager)
        {
            if (!container.IsRegistered<T>(name))
            {
                container.RegisterType(typeof(T), typeof(TT), name, manager);
            }
        }

        internal static void DoDefaultRegistrations(UnityContainer container)
        {
            container.RegisterIfNever<IApiManager,ApiManager>();
            
            container.RegisterIfNever<IApiStandartResponce, ApiStandartResponce>(new NewInstanceLifetimeManager());
            container.RegisterIfNever<IApiResponceFilter, ApiSmartListResponceFilter>("smartfilter", new NewInstanceLifetimeManager());

            container.RegisterIfNever<IApiMethodCall, ApiMethodCall>(new NewInstanceLifetimeManager());
            container.RegisterIfNever<IApiArgumentBuilder, ApiArgumentBuilder>();

            //Serializers
            container.RegisterIfNever<IApiSerializer, JsonNetSerializer>("json.net.serializer");

            //Responders
            container.RegisterIfNever<IApiResponder, ContentResponder>("content_responder");
            container.RegisterIfNever<IApiResponder, DirectResponder>("direct_responder");
            container.RegisterIfNever<IApiResponder, SerializerResponder>("serialzer");

            container.RegisterIfNever<IApiMethodInvoker, ApiSimpleMethodInvoker>();
            container.RegisterIfNever<IApiStoragePath, ApiStoragePath>();
            container.RegisterIfNever<IApiKeyValueStorage, ApiKeyValueInMemoryStorage>();

            container.RegisterIfNever<IApiRouteConfigurator, ApiRouteConfigurator>();
            container.RegisterIfNever<IApiRouteRegistrator, ApiRouteRegistrator>("rest");
            container.RegisterIfNever<IApiRouteRegistrator, ApiBatchRouteRegitrator>("batch");
            container.RegisterIfNever<IApiRouteRegistrator, ApiAccessControlRouteRegistrator>("access");


            container.RegisterIfNever<IApiHttpHandler, ApiHttpHandler>(new NewInstanceLifetimeManager());
            container.RegisterIfNever<IApiRouteHandler, ApiRouteHandler>(new NewInstanceLifetimeManager());
            container.RegisterType<ApiContext>(new ApiContextLifetimeManager());
        }        
    }
}