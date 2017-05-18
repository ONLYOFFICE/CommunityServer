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
using System.Web.Configuration;
using ASC.Core.Tenants;
using ASC.VoipService;
using Microsoft.AspNet.SignalR.Client;

namespace ASC.Web.CRM.Classes
{
    public class SignalRHelper
    {
        private readonly string hubUrl;
        private readonly string numberId;
        private const string hubName = "voip";

        public Tenant Tenant { get; set; }
        public Guid CurrentAccountId { get; set; }

        public SignalRHelper(Tenant tenant, Guid currentAccount, string numberId)
        {
            hubUrl = WebConfigurationManager.AppSettings["web.hub"];

            if (string.IsNullOrEmpty(hubUrl) || hubUrl == "/")
            {
                hubUrl = "http://localhost:9899/";
            }

            this.numberId = numberId.TrimStart('+');
            Tenant = tenant;
            CurrentAccountId = currentAccount.Equals(Guid.Empty) || currentAccount.Equals(ASC.Core.Configuration.Constants.Guest.ID) ? tenant.OwnerId : currentAccount;
        }

        public void Enqueue(string call, string agent)
        {
            Invoke("enqueue", call, agent);
        }

        public void Incoming(string call, string agent)
        {
            Invoke("incoming", call, agent);
        }

        public void MissCall(string call, string agent)
        {
            Invoke("miss", call, agent);
        }

        public Tuple<Agent, bool> GetAgent(List<Guid> contactsResponsibles)
        {
            return Invoke<Tuple<Agent, bool>>("GetAgent", contactsResponsibles);
        }

        private void Invoke(string method, params object[] args)
        {
            var hubConnection = new HubConnection(hubUrl, GetQueryString());
            hubConnection.Headers.Add("voipConnection", "true");

            var voipHubProxy = hubConnection.CreateHubProxy(hubName);
            hubConnection.Start().ContinueWith(r =>
                {
                    voipHubProxy.Invoke(method, args).Wait();
                    hubConnection.Stop();
                });
        }

        private T Invoke<T>(string method, params object[] args)
        {
            using (var hubConnection = new HubConnection(hubUrl, GetQueryString()))
            {
                hubConnection.Headers.Add("voipConnection", "true");

                var voipHubProxy = hubConnection.CreateHubProxy(hubName);
                return hubConnection.Start().ContinueWith(r =>
                {
                    return voipHubProxy.Invoke<T>(method, args).Result;
                }).Result;
            }
        }

        private string GetQueryString()
        {
            var token = Common.Utils.Signature.Create(string.Join(",", Tenant.TenantId, CurrentAccountId, Tenant.TenantAlias));
            return string.Format("token={0}&numberId={1}", token, numberId);
        }
    }
}