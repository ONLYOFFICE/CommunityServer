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


using ASC.Common.Data;
using ASC.Common.DependencyInjection;
using Autofac;

namespace ASC.Web.Files.Core
{
    public static class DIHelper
    {
        internal static IContainer Builder { get; set; }
        private static bool isRegistered;
        private static readonly object Locker = new object();

        public static void Register()
        {
            if (isRegistered) return;

            lock (Locker)
            {
                if (isRegistered) return;

                var container = AutofacConfigLoader.Load("files");

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
            Register();

            var scope = Builder.BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            scope.Resolve<IDbManager>();

            return scope;
        }
    }
}