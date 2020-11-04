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
using System.Collections.Generic;
using System.Linq;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core.Utility
{
    public static class SearchHandlerManager
    {
        private static readonly List<ISearchHandlerEx> handlers = new List<ISearchHandlerEx>();

        public static void Registry(ISearchHandlerEx handler)
        {
            lock (handlers)
            {
                if (handler != null && handlers.All(h => h.GetType() != handler.GetType()))
                {
                    handlers.Add(handler);
                }
            }
        }

        public static void UnRegistry(ISearchHandlerEx handler)
        {
            lock (handlers)
            {
                if (handler != null)
                {
                    handlers.RemoveAll(h => h.GetType() == handler.GetType());
                }
            }
        }

        public static List<ISearchHandlerEx> GetAllHandlersEx()
        {
            lock (handlers)
            {
                return handlers.ToList();
            }
        }

        public static List<ISearchHandlerEx> GetHandlersExForProduct(Guid productID)
        {
            lock (handlers)
            {
                return handlers
                    .Where(h => h.ProductID.Equals(productID) || h.ProductID.Equals(Guid.Empty))
                    .ToList();
            }
        }

        public static List<ISearchHandlerEx> GetHandlersExForProducts(Guid[] productIDs)
        {
            lock (handlers)
            {
                return handlers
                    .Where(h => productIDs.Contains(h.ProductID) || h.ProductID.Equals(Guid.Empty))
                    .ToList();
            }
        }

        public static List<ISearchHandlerEx> GetHandlersExForProductModule(Guid productID, Guid moduleID)
        {
            var searchHandlers = productID.Equals(Guid.Empty)
                                     ? GetAllHandlersEx()
                                     : GetHandlersExForProduct(productID);

            lock (searchHandlers)
            {
                return moduleID.Equals(Guid.Empty)
                           ? searchHandlers.ToList()
                           : searchHandlers.FindAll(x => x.ModuleID == moduleID).ToList();
            }
        }

        public static List<ISearchHandlerEx> GetHandlersExForProductModule(Guid[] productIDs)
        {
            return GetHandlersExForProducts(productIDs);
        }
    }

    public abstract class BaseSearchHandlerEx : ISearchHandlerEx
    {
        public abstract ImageOptions Logo { get; }

        public abstract string SearchName { get; }

        public virtual IItemControl Control
        {
            get { return null; }
        }

        public virtual Guid ProductID
        {
            get { return Guid.Empty; }
        }

        public virtual Guid ModuleID
        {
            get { return Guid.Empty; }
        }

        public abstract SearchResultItem[] Search(string text);
    }
}