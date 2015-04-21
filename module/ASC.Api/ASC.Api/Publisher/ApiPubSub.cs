/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASC.Api.Interfaces;
using ASC.Collections;

namespace ASC.Api.Publisher
{
    public class ApiPubSub : IApiPubSub
    {
        private readonly SynchronizedDictionary<string, IList<DataHandler>> _handlers = new SynchronizedDictionary<string, IList<DataHandler>>();

        public void PublishDataForKey(string key, object data)
        {
            if (_handlers.ContainsKey(key))
            {
                IList<DataHandler> handlers = null;
                using (_handlers.GetWriteLock())
                {
                    handlers = new List<DataHandler>(_handlers[key]); //Copy
                    _handlers[key].Clear();
                }
                //Call
                ThreadPool.QueueUserWorkItem(x =>
                                                 {
                                                     foreach (var handler in handlers)
                                                     {
                                                         handler.OnDataAvailible(data);
                                                     }
                                                     handlers.Clear();
                                                 });
            }
        }

        public void SubscribeForKey(string key, DataAvailibleDelegate dataAvailibleDelegate, object userObject)
        {
            if (_handlers.ContainsKey(key))
            {
                _handlers[key].Remove(new DataHandler(userObject, dataAvailibleDelegate));
            }
            else
            {
                _handlers.Add(key, new List<DataHandler>());
            }
            _handlers[key].Add(new DataHandler(userObject, dataAvailibleDelegate));

        }

        public void UnsubscribeForKey(string key, DataAvailibleDelegate dataAvailibleDelegate, object userObject)
        {
            //Dumb method
        }
    }
}