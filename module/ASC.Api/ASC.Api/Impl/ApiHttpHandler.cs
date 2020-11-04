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
            catch (ApiArgumentMismatchException e)
            {
                SetError(context, e, HttpStatusCode.BadRequest, "Invalid arguments");
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