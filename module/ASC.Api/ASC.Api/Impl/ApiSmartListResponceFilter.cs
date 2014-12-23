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

#region usings

using System;
using System.Collections;
using ASC.Api.Collections;
using ASC.Api.Interfaces;

#endregion

namespace ASC.Api.Impl
{
    public class ApiSmartListResponceFilter : IApiResponceFilter
    {

        #region IApiResponceFilter Members

        public object FilterResponce(object responce, ApiContext context)
        {
            if (responce != null && !context.FromCache)
            {
                ISmartList smartList = null;
                var type = responce.GetType();
                if (responce is ISmartList)
                {
                    smartList = responce as ISmartList;
                }
                else if (Utils.Binder.IsCollection(type) && !typeof(IDictionary).IsAssignableFrom(type))
                {
                    try
                    {
                        var elementType = Utils.Binder.GetCollectionType(type);
                        var smartListType = SmartListFactory.GetSmartListType().MakeGenericType(elementType);
                        smartList = Activator.CreateInstance(smartListType, (IEnumerable)responce) as ISmartList;
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                if (smartList != null)
                {
                    return TransformList(context, smartList);
                }
            }
            return responce;
        }

        private static object TransformList(ApiContext context, ISmartList smartList)
        {
            bool getTotalCount = context.SpecifiedCount < smartList.Count && !context.TotalCount.HasValue;/*We have already more items than needed and no one set totalcount*/

            smartList.TakeCount = context.SpecifiedCount;
            smartList.StartIndex = context.StartIndex;
            smartList.IsDescending = context.SortDescending;
            smartList.SortBy = context.SortBy;
            smartList.FilterBy = context.FilterBy;
            smartList.FilterOp = context.FilterOp;
            smartList.FilterValue = context.FilterValues;
            smartList.UpdatedSince = context.UpdatedSince;
            smartList.FilterType = context.FilterToType;
            var list= smartList.Transform(getTotalCount);
            if (getTotalCount)
            {
                context.TotalCount = smartList.TotalCount;
            }
            return list;
        }

        #endregion
    }
}