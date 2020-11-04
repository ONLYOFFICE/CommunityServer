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


using System.Linq;
using System.Web;
using System.Web.Http;

namespace ASC.CdnCheck.Controllers
{
    public class CdnController : ApiController
    {
        [HttpGet]
        public void Error(int status, string url)
        {
            var logger = log4net.LogManager.GetLogger("ASC");
            logger.Debug("");
            logger.DebugFormat("status:{0}", status);

            foreach (var header in Request.Headers)
            {
                logger.DebugFormat("{0}: {1}", header.Key, header.Value.Aggregate((a,b)=> string.Format("{0},{1}",a,b)));
            }

            logger.DebugFormat("host address {0}", HttpContext.Current.Request.UserHostAddress);
            logger.DebugFormat("host name {0}", HttpContext.Current.Request.UserHostName);
        }
    }
}