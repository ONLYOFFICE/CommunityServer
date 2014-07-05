/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System.Security;
using ASC.Api.Exceptions;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Routing;

namespace ASC.Api.Impl
{
    public class ApiHttpHandler : ApiHttpHandlerBase
    {
        #region IApiHttpHandler Members


        public ApiHttpHandler(RouteData routeData)
            : base(routeData)
        {

        }

        protected override void DoProcess(HttpContextBase context)
        {
            Log.Debug("strating request. context: '{0}'", ApiContext);

            //Neeeded to rollback errors
            context.Response.Buffer = true;
            context.Response.BufferOutput = true;

            try
            {
                Log.Debug("method invoke");
                ApiResponce.Count = ApiContext.Count;
                ApiResponce.StartIndex = ApiContext.StartIndex;

                if (Method != null)
                {
                    var responce = ApiManager.InvokeMethod(Method, ApiContext);
                    if (responce is Exception)
                    {
                        SetError(context, (Exception)responce, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        // success
                        PostProcessResponse(context, responce);
                    }
                }
                else
                {
                    SetError(context, new MissingMethodException("Method not found"), HttpStatusCode.NotFound);
                }
            }
            catch (TargetInvocationException targetInvocationException)
            {
                if (targetInvocationException.InnerException is ItemNotFoundException)
                {
                    SetError(context, targetInvocationException.InnerException, HttpStatusCode.NotFound, "The record could not be found");
                }
                else if (targetInvocationException.InnerException is ArgumentException)
                {
                    SetError(context, targetInvocationException.InnerException, HttpStatusCode.BadRequest, "Invalid arguments");
                }
                else if (targetInvocationException.InnerException is SecurityException)
                {
                    SetError(context, targetInvocationException.InnerException, HttpStatusCode.Forbidden, "Access denied");
                }
                else
                {
                    SetError(context, targetInvocationException.InnerException, HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception e)
            {
                SetError(context, e, HttpStatusCode.InternalServerError);
            }

            Exception responseError;
            try
            {
                RespondTo(Method, context);
                return;
            }
            catch (ThreadAbortException e)
            {
                //Do nothing. someone killing response
                Log.Error(e, "thread aborted. response not sent");
                return;
            }
            catch (HttpException exception)
            {
                responseError = exception;
                SetError(context, exception, (HttpStatusCode)exception.GetHttpCode());//Set the code of throwed exception
            }
            catch (Exception exception)
            {
                responseError = exception;
                SetError(context, exception, HttpStatusCode.InternalServerError);
            }
            Log.Error(responseError, "error happened while sending response. can't be here");
            RespondTo(Method, context);//If we got there then something went wrong
        }

        #endregion
    }
}