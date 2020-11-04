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