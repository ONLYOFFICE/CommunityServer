/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Web.Core.Subscriptions;
using ASC.Notify.Model;

namespace ASC.Web.Studio.Core.Notify
{
    internal class StudioSubscriptionManager : ISubscriptionManager
    {
        private static StudioSubscriptionManager _instance = new StudioSubscriptionManager();

        public static StudioSubscriptionManager Instance
        {
            get { return _instance; }
        }

        private StudioSubscriptionManager()
        { }

        #region ISubscriptionManager Members

        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            return new List<SubscriptionObject>();
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var types = new List<SubscriptionType>();
            types.Add(new SubscriptionType()
            {
                ID = new Guid("{148B5E30-C81A-4ff8-B749-C46BAE340093}"),
                Name = Resources.Resource.WhatsNewSubscriptionName,
                NotifyAction = Constants.ActionSendWhatsNew,
                Single = true
            });

            var astype = new SubscriptionType()
            {
                ID = new Guid("{A4FFC01F-BDB5-450e-88C4-03FED17D67C5}"),
                Name = Resources.Resource.AdministratorNotifySenderTypeName,
                NotifyAction = Constants.ActionSendWhatsNew,
                Single = false
            };
            
            types.Add(astype);

            return types;
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return StudioNotifyService.Instance.source.GetSubscriptionProvider(); }
        }

        #endregion
    }
}
