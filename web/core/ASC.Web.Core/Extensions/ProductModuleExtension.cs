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
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core
{
    public static class ProductModuleExtension
    {
        
        
        public static string GetSmallIconAbsoluteURL(this IModule module)
        {
            if (module == null || module.Context == null || String.IsNullOrEmpty(module.Context.SmallIconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(module.Context.SmallIconFileName, module.ID);
        }

        public static string GetSmallIconAbsoluteURL(this IProduct product)
        {
            if (product == null || product.Context == null || String.IsNullOrEmpty(product.Context.SmallIconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(product.Context.SmallIconFileName, product.ID);
        }

        public static string GetIconAbsoluteURL(this IModule module)
        {
            if (module == null || module.Context == null || String.IsNullOrEmpty(module.Context.IconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(module.Context.IconFileName, module.ID);
        }

        public static string GetIconAbsoluteURL(this IProduct product)
        {
            if (product == null || product.Context == null || String.IsNullOrEmpty(product.Context.IconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(product.Context.IconFileName, product.ID);
        }
    }
}
