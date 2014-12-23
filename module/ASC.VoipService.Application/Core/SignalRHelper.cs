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