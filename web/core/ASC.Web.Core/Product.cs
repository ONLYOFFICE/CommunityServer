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


using System;
using System.IO;

using ASC.Data.Storage;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core
{
    [WebZoneAttribute(WebZoneType.TopNavigationProductList | WebZoneType.StartProductList)]
    public abstract class Product : IProduct
    {
        public abstract Guid ProductID { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string StartURL { get; }

        public abstract string HelpURL { get; }
        public virtual string WarmupURL { get { return string.Concat(StartURL, "Warmup.aspx"); } }

        public abstract string ProductClassName { get; }

        public abstract bool Visible { get; }

        public abstract void Init();

        public abstract ProductContext Context { get; }

        public virtual void Shutdown() { }

        WebItemContext IWebItem.Context { get { return ((IProduct)this).Context; } }

        Guid IWebItem.ID { get { return ProductID; } }

        public virtual ClientScriptLocalization ClientScriptLocalization { get; protected set; }

        public string GetResourcePath(string relativePath)
        {
            return WebPath.GetPath(Path.Combine(StartURL, relativePath));
        }
    }
}
