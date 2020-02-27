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


using System.Security;
using ASC.Api.Exceptions;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Routing;
using ASC.Api.Interfaces;
using Autofac;

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
            Log.DebugFormat("strating request. context: '{0}'", ApiContext);

            //Neeeded to rollback errors
            context.Response.Buffer = true;
            context.Response.BufferOutput = true;

            IApiEntryPoint instance = null;

            try
            {
                Log.Debug("method invoke");
                ApiResponce.Count = ApiContext.Count;
                ApiResponce.StartIndex = ApiContext.StartIndex;

                if (Method != null)
                {
                    if (!string.IsNullOrEmpty(Method.Name))
                    {
                        instance = Container.ResolveNamed<IApiEntryPoint>(Method.Name, new TypedParameter(typeof(ApiContext), ApiContext));
                    }
                    else
                    {
                        instance = Container.Resolve<IApiEntryPoint>();
                    }

                    var responce = ApiManager.InvokeMethod(Method, ApiContext, instance);
                    if (responce is Exception)
                    {
                        SetError(context, (Exception) responce, HttpStatusCode.InternalServerError);
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
                else if (targetInvocationException.InnerException is InvalidOperationException)
                {
                    SetError(context, targetInvocationException.InnerException, HttpStatusCode.Forbidden);
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
                Log.Error("thread aborted. response not sent", e);
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
            Log.Error("error happened while sending response. can't be here", responseError);
            RespondTo(Method, context);//If we got there then something went wrong
        }

        #endregion
    }
}