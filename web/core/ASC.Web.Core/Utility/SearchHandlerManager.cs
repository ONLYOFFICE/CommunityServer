/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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