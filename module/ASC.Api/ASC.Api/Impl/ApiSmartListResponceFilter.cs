/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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