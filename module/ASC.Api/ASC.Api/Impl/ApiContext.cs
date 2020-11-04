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
using System.Web.Routing;
using ASC.Api.Interfaces;
using Autofac;

namespace ASC.Api.Impl
{
    public class ApiContext:ICloneable
    {
        public RequestContext RequestContext { get; set; }

        public ApiContext(RequestContext requestContext) : this(requestContext, null)
        {
        }

        public ApiContext(RequestContext requestContext,IApiConfiguration apiConfiguration)
        {
            if (apiConfiguration==null)
                apiConfiguration = ApiSetup.Builder.Resolve<IApiConfiguration>();

            if (requestContext == null) return;
            RequestContext = requestContext;

            //!!! Register in HttpContext. Needed for lifetime manager
            RequestContext.HttpContext.Items["_apiContext"] = this;

            Count = 0;
            //Try parse values
            string count = GetRequestValue("count");
            ulong countParsed;
            if (!string.IsNullOrEmpty(count) && ulong.TryParse(count,out countParsed))
            {
                //Count specified and valid
                SpecifiedCount = (long)Math.Max(0,Math.Min(apiConfiguration.ItemsPerPage, countParsed));
            }
            else
            {
                SpecifiedCount = Math.Max(0,apiConfiguration.ItemsPerPage);
            }
            Count = SpecifiedCount + 1;//NOTE: +1 added to see if it's last page


            string startIndex = GetRequestValue("startIndex");
            long startIndexParsed;
            if (startIndex != null && long.TryParse(startIndex,out startIndexParsed))
            {
                StartIndex =Math.Max(0,startIndexParsed);
                SpecifiedStartIndex = StartIndex;
            }

            string sortOrder = GetRequestValue("sortOrder");
            if ("descending".Equals(sortOrder))
            {
                SortDescending = true;
            }

            FilterToType = GetRequestValue("type");
            SortBy = GetRequestValue("sortBy");
            FilterBy = GetRequestValue("filterBy");
            FilterOp = GetRequestValue("filterOp");
            FilterValue = GetRequestValue("filterValue");
            FilterValues = GetRequestArray("filterValue");
            Fields = GetRequestArray("fields");

            string updatedSince = GetRequestValue("updatedSince");
            if (updatedSince != null)
            {
                UpdatedSince = Convert.ToDateTime(updatedSince);
            }
        }

        public string[] Fields { get; set; }

        private string[] GetRequestArray(string key)
        {
            if (RequestContext.HttpContext.Request.QueryString != null)
            {
                var values = RequestContext.HttpContext.Request.QueryString.GetValues(key + "[]");
                if (values != null)
                    return values;
                
                values = RequestContext.HttpContext.Request.QueryString.GetValues(key);
                if (values!=null)
                {
                    if (values.Length==1) //If it's only one element
                    {
                        //Try split
                        if (!string.IsNullOrEmpty(values[0]))
                            return values[0].Split(',');
                    }
                    return values;
                }
            }
            return null;
        }

        public string GetRequestValue(string key)
        {
            var reqArray = GetRequestArray(key);
            return reqArray != null ? reqArray.FirstOrDefault() : null;
        }

        public string[] FilterValues { get; set; }

        /// <summary>
        /// Filters responce to specific type from request parameter "type"
        /// </summary>
        /// <remarks>
        /// The type name is retrieved from [DataContractAttribute] name
        /// </remarks>
        public string FilterToType { get; set; }

        /// <summary>
        /// Gets count to get item from collection. Request parameter "count"
        /// </summary>
        /// <remarks>
        /// Don't forget to call _context.SetDataPaginated() to prevent SmartList from filtering response if you fetch data from DB with TOP & COUNT
        /// </remarks>
        public long Count { get; set; }
        
        /// <summary>
        /// Gets start index to get item from collection. Request parameter "startIndex"
        /// </summary>
        /// <remarks>
        /// Don't forget to call _context.SetDataPaginated() to prevent SmartList from filtering response if you fetch data from DB with TOP & COUNT
        /// </remarks>
        public long StartIndex { get; set; }

        internal long SpecifiedStartIndex { get; set; }

        /// <summary>
        /// Gets field to sort by from request parameter "sortBy"
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Gets field to filter from request parameter "filterBy"
        /// </summary>
        public string FilterBy { get; set; }
        
        /// <summary>
        /// Gets filter operation from request parameter "filterOp"
        /// can be one of the following:"contains","equals","startsWith","present"
        /// </summary>
        public string FilterOp { get; set; }

        /// <summary>
        /// Gets value to filter from request parameter "filterValue"
        /// </summary>
        public string FilterValue { get; set; }

        /// <summary>
        /// Sort direction. From request parameter "sortOrder" can be "descending" or "ascending"
        /// Like ...&sortOrder=descending&...
        /// </summary>
        public bool SortDescending { get; set; }
        
        /// <summary>
        /// Gets value to filter from request parameter "updatedSince"
        /// </summary>
        public DateTime UpdatedSince { get; set; }

        public bool FromCache { get; set; }

        private readonly List<Type> _knowntype = new List<Type>();

        internal long SpecifiedCount { get; private set; }

        /// <summary>
        /// Set mark that data is already paginated and additional filtering is not needed
        /// </summary>
        public ApiContext SetDataPaginated()
        {
            //Count = 0;//We always ask for +1 count so smart list should cut it
            StartIndex = 0;
            return this;
        }

        public ApiContext SetDataSorted()
        {
            SortBy = string.Empty;
            return this;
        }
        
        public ApiContext SetDataFiltered()
        {
            FilterBy = string.Empty;
            FilterOp = string.Empty;
            FilterValue = string.Empty;
            return this;
        }

        public ApiContext SetTotalCount(long totalCollectionCount)
        {
            TotalCount = totalCollectionCount;
            return this;
        }

        public long? TotalCount { get; set; }


        public void RegisterType(Type type)
        {
            if (!_knowntype.Contains(type))
            {
                _knowntype.Add(type);
            }
        }

        public void RegisterTypes(IEnumerable<Type> types)
        {
            _knowntype.AddRange(types);

        }

        internal IEnumerable<Type> GetKnownTypes()
        {
            return _knowntype.Distinct();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("C:{0},S:{1},So:{2},Sd:{3},Fb;{4},Fo:{5},Fv:{6},Us:{7},Ftt:{8}", Count, StartIndex,
                                 SortBy, SortDescending, FilterBy, FilterOp, FilterValue, UpdatedSince.Ticks, FilterToType);
        }
    }
}