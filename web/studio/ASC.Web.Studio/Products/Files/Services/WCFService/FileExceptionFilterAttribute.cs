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

using ASC.Web.Files.Resources;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.Filters;

namespace ASC.Web.Files.Services.WCFService
{
    class FileExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null && actionExecutedContext.Response == null)
            {
                var fileError = new FileError(actionExecutedContext.Exception);
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, fileError);
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