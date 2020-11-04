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
using System.Linq;

namespace ASC.Api.Routing
{
    public class RouteCallInfo
    {
        public int Tid { get; set; }
        public string Url { get; set; }

        public string Method { get; set; }

        public Dictionary<string, object> Params { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} T:{2},{3}", Method.ToUpper(),Url,Tid,string.Join(",",Params.Select(x=>string.Format("{0}={1}",x.Key,x.Value)).ToArray()));
        }
    }
}