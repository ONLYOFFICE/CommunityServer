/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System.Collections.Generic;
using System.Web.Routing;
using ASC.Api.Interfaces;
using ASC.Common.DependencyInjection;
using ASC.Common.Logging;
using Autofac;

namespace ASC.Api
{
    public static class ApiSetup
    {
        private static object locker = new object();

        private static volatile bool initialized = false;

        public static IContainer Builder { get; private set; }


        static ApiSetup()
        {
        }


        public static void Init()
        {
            if (!initialized)
            {
                lock (locker)
                {
                    if (!initialized)
                    {
                        var container = AutofacConfigLoader.Load("api");

                        container.Register(c => LogManager.GetLogger("ASC.Api"))
                            .As<ILog>()
                            .SingleInstance();
                        
                        container.Register(c => c.Resolve<IApiRouteConfigurator>().RegisterEntryPoints())
                            .As<IEnumerable<IApiMethodCall>>()
                            .SingleInstance();

                        Builder = container.Build();

                        ConfigureEntryPoints();
                        RegisterRoutes();

                        initialized = true;
                    }
                }
            }
        }

        private static void RegisterRoutes()
        {
            var registrators = Builder.Resolve<IEnumerable<IApiRouteRegistrator>>();
            foreach (var registrator in registrators)
            {
                registrator.RegisterRoutes(RouteTable.Routes);
            }
        }

        private static void ConfigureEntryPoints()
        {
            //Do boot stuff
            Builder.Resolve<IApiRouteConfigurator>();

            //Do boot auto search
            var boot = Builder.Resolve<IEnumerable<IApiBootstrapper>>();
            foreach (var apiBootstrapper in boot)
            {
                apiBootstrapper.Configure();
            }
        }
    }
}