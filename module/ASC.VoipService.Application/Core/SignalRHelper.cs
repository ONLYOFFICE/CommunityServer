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
using System.Configuration;
using System.Threading.Tasks;
using ASC.Core;
using ASC.Core.Tenants;
using Microsoft.AspNet.SignalR.Client;

namespace ASC.VoipService.Application.Core
{
    public class SignalRHelper
    {
        private readonly string hubUrl;
        private readonly string numberId;
        private const string hubName = "voip";

        public Tenant Tenant { get; set; }
        public Guid CurrentAccountId { get; set; }

        public SignalRHelper(string domain, string currentAccount, string numberId)
        {
            hubUrl = ConfigurationManager.AppSettings["hubUrl"] ?? "http://localhost:8884/";
            this.numberId = numberId;

            var multiRegionHostedSolution = new MultiRegionHostedSolution("teamlabsite");
            Tenant = multiRegionHostedSolution.GetTenant(domain);
            CurrentAccountId = !string.IsNullOrEmpty(currentAccount) ? new Guid(currentAccount) : Tenant.OwnerId;
        }

        public void ChangeAgentStatus(int status)
        {
            Invoke("status", status);
        }

        public void Enqueue(object call)
        {
            Invoke("enqueue", call);
        }

        public void Incoming(object call, string agent)
        {
            Invoke("incoming", call, agent);
        }

        public void MissCall(object call)
        {
            Invoke("miss", call);
        }

        public void VoiceMail(object call)
        {
            Invoke("mail", call);
        }

        public void Start()
        {
            Invoke("start");
        }

        public void End()
        {
            Invoke("end");
        }

        private void Invoke(string method, params object[] args)
        {
            var token = Common.Utils.Signature.Create(string.Join(",", Tenant.TenantId, CurrentAccountId, Tenant.TenantAlias));
            var hubConnection = new HubConnection(hubUrl, string.Format("token={0}&numberId={1}", token, numberId));
            hubConnection.Headers.Add("voipConnection", "true");

            var voipHubProxy = hubConnection.CreateHubProxy(hubName);
            hubConnection.Start().ContinueWith(r =>
                {
                    voipHubProxy.Invoke(method, args).Wait();
                    hubConnection.Stop();
                });
        }
    }
}