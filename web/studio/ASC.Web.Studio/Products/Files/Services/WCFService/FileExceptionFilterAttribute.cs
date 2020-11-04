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
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.Filters;
using ASC.Common.Logging;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Services.WCFService
{
    class FileExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Files");


        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null && actionExecutedContext.Response == null)
            {
                var fileError = new FileError(actionExecutedContext.Exception);
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, fileError);
            }
            LogException(actionExecutedContext.Exception);
        }


        [Conditional("DEBUG")]
        private void LogException(Exception err)
        {
            while (err != null)
            {
                log.Error(err);
                err = err.InnerException;
            }
        }


        [DataContract(Name = "error", Namespace = "")]
        class FileError
        {
            [DataMember(Name = "Detail")]
            public string Detail { get; set; }

            [DataMember(Name = "message")]
            public string Message { get; set; }

            [DataMember(Name = "inner")]
            public FileErrorInner Inner { get; set; }

            [DataContract(Name = "error", Namespace = "")]
            internal class FileErrorInner
            {
                [DataMember(Name = "message")]
                public string Message { get; set; }

                [DataMember(Name = "type")]
                public string Type { get; set; }

                [DataMember(Name = "source")]
                public string Source { get; set; }

                [DataMember(Name = "stack")]
                public string Stack { get; set; }
            }

            public FileError()
            {
            }

            public FileError(Exception error)
            {
                Detail = error.Message;
                Message = FilesCommonResource.ErrorMassage_BadRequest;
                Inner = new FileErrorInner
                {
                    Message = error.Message,
                    Type = error.GetType().FullName,
                    Source = error.Source ?? string.Empty,
                    Stack = error.StackTrace ?? string.Empty,
                };
            }
        }
    }
}