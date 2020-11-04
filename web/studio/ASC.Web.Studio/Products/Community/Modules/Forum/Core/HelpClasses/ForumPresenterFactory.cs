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
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ASC.Forum
{
    public class ForumPresenterFactory : IPresenterFactory
    {
        public IPresenter GetPresenter<T>() where T : class
        {
            IPresenter presenter = null;            

            if (typeof(T).Equals(typeof(ISecurityActionView)))
                presenter = new SecurityActionPresenter();
         
            else if (typeof(T).Equals(typeof(INotifierView)))
                presenter = new NotifierPresenter();

            else if (typeof(T).Equals(typeof(ISubscriberView)))
                presenter = new SubscriberPresenter();

            else if (typeof(T).Equals(typeof(ISubscriptionGetcherView)))
                presenter = new SubscriptionGetcherPresenter();

            return presenter;
        }    
    }
}
