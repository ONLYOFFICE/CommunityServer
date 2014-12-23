/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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