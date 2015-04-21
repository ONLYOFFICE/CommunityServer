using System;
using System.IO;
using System.Threading;

using AppLimit.CloudComputing.SharpBox.Common;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using System.Collections.Generic;

namespace AppLimit.CloudComputing.SharpBox
{
    public partial class CloudStorage
    {             
        #region Async Functions

        internal class BackgroundRequest : AsyncObjectRequest
        {
            public Object OperationResult;
            public Object OperationParameter;
        }

        internal class OpenRequest : BackgroundRequest 
        {
            public ICloudStorageConfiguration config;
            public ICloudStorageAccessToken token;            
        }

        /// <summary>
        /// This method implements a asyncallback for the open
        /// request, which is describe in BeginOpenRequest
        /// </summary>
        /// <param name="state">A reference to the start object</param>
        private void OpenRequestCallback(object state)
        {
            // cast the request 
            OpenRequest req = state as OpenRequest;

            try
            {
                // perform the request                                
                req.OperationResult = Open(req.config, req.token);                
            }
            catch (Exception e)
            {
                // failure to login
                var openRequest = req.result.AsyncState as BackgroundRequest;
                openRequest.OperationResult = null;
                openRequest.errorReason = e;
            }

            // call the async callback
            req.callback(req.result);
        }   

        /// <summary>
        /// Starts the async open request
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="configuration"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IAsyncResult BeginOpenRequest(AsyncCallback callback, ICloudStorageConfiguration configuration, ICloudStorageAccessToken token)
        {
            // build the request data structure
            OpenRequest request = new OpenRequest();
            request.callback = callback;
            request.result = new AsyncResultEx(request);
            request.config = configuration;
            request.token = token;

            // add to threadpool
            ThreadPool.QueueUserWorkItem(OpenRequestCallback, request);

            // return the result
            return request.result;
        }

        /// <summary>
        /// Finishs the async open request
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken EndOpenRequest(IAsyncResult asyncResult)
        {
            OpenRequest req = asyncResult.AsyncState as OpenRequest;
            return req.OperationResult as ICloudStorageAccessToken;   
        }


        /// <summary>
        /// This method implements a asyncallback for the get childs
        /// request, which is describe in BeginGetChildsRequest
        /// </summary>
        /// <param name="state"></param>
        private void GetChildsRequestCallback(object state)
        {
            // cast the request 
            BackgroundRequest req = state as BackgroundRequest;

            try
            {
                List<ICloudFileSystemEntry> retList = new List<ICloudFileSystemEntry>();

                foreach (ICloudFileSystemEntry e in req.OperationParameter as ICloudDirectoryEntry)
                {
                    retList.Add(e);
                }

                req.OperationResult = retList;
            }
            catch (Exception e)
            {
                // failure to login
                var openRequest = req.result.AsyncState as BackgroundRequest;
                openRequest.OperationResult = null;
                openRequest.errorReason = e;
            }

            // call the async callback
            req.callback(req.result);
        }

        /// <summary>
        /// Starts the asyn get childs call
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetChildsRequest(AsyncCallback callback, ICloudDirectoryEntry parent)
        {
            // build the request data structure
            BackgroundRequest request = new BackgroundRequest();
            request.callback = callback;
            request.result = new AsyncResultEx(request);
            request.OperationParameter = parent;

            // add to threadpool
            ThreadPool.QueueUserWorkItem(GetChildsRequestCallback, request);

            // return the result
            return request.result;            
        }

        /// <summary>
        /// Finishes the async get childs call
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public List<ICloudFileSystemEntry> EndGetChildsRequest(IAsyncResult asyncResult)
        {
            BackgroundRequest req = asyncResult.AsyncState as BackgroundRequest;            
            return req.OperationResult as List<ICloudFileSystemEntry>;
        }


        /// <summary>
        /// This method implements a asyncallback for the getroot
        /// request, which is describe in BeginGetRootRequest
        /// </summary>
        /// <param name="state"></param>
        private void GetRootRequestCallback(object state)
        {
            // cast the request 
            BackgroundRequest req = state as BackgroundRequest;

            try
            {
                req.OperationResult = GetRoot();
            }
            catch (Exception e)
            {
                // failure to login
                var openRequest = req.result.AsyncState as BackgroundRequest;
                openRequest.OperationResult = null;
                openRequest.errorReason = e;
            }

            // call the async callback
            req.callback(req.result);
        }

        /// <summary>
        /// Starts the asyn getRoot call
        /// </summary>
        /// <param name="callback"></param>        
        /// <returns></returns>
        public IAsyncResult BeginGetRootRequest(AsyncCallback callback)
        {
            // build the request data structure
            BackgroundRequest request = new BackgroundRequest();
            request.callback = callback;
            request.result = new AsyncResultEx(request);
            request.OperationParameter = null;

            // add to threadpool
            ThreadPool.QueueUserWorkItem(GetRootRequestCallback, request);

            // return the result
            return request.result;
        }

        /// <summary>
        /// Finishes the async getRoot call
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public ICloudDirectoryEntry EndGetRootRequest(IAsyncResult asyncResult)
        {
            BackgroundRequest req = asyncResult.AsyncState as BackgroundRequest;
            return req.OperationResult as ICloudDirectoryEntry;
        }
    
       
        #endregion
    }
}
