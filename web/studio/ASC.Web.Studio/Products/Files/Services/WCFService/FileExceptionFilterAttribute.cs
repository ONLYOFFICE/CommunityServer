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