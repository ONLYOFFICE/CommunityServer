/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Server.Utils;
using System;
using System.Collections.Generic;

namespace ASC.Xmpp.Server.Streams
{
	public class XmppStream
	{

		private string ns;

		private string language;

		private string version;

		private string domain;

		private List<string> resources;


		public string Id
		{
			get;
			private set;
		}

		public string ConnectionId
		{
			get;
			private set;
		}

		public bool Authenticated
		{
			get;
			set;
		}

		public bool Connected
		{
			get;
			set;
		}

		public string Namespace
		{
			get { return ns; }
			set { ns = Nameprep(value); }
		}

		public string Language
		{
			get { return language; }
			set { language = Nameprep(value); }
		}

		public string Version
		{
			get { return version; }
			set { version = Nameprep(value); }
		}

		public string User
		{
			get;
			set;
		}


		public string Domain
		{
			get { return domain; }
			set { domain = Nameprep(value); }
		}

		public IList<string> Resources
		{
			get
            {
                lock (resources)
                {
                    return resources.AsReadOnly();
                }
            }
		}

		public bool MultipleResources
		{
			get { return 1 < Resources.Count; }
		}

	    public XmppStream(string connectionId)
		{
			if (string.IsNullOrEmpty(connectionId)) throw new ArgumentNullException("connectionId");

			Id = UniqueId.CreateNewId();
			ConnectionId = connectionId;
			Version = "1.0";
			resources = new List<string>();
		}

		public void Authenticate(string userName)
		{
			User = !string.IsNullOrEmpty(userName) ? Stringprep.NodePrep(userName) : null;
			Authenticated = true;
		}

		public bool JidBinded(Jid jid)
		{
			if (jid == null) throw new ArgumentNullException("jid");
			if (jid.User != User) return false;
			if (jid.Server != Domain) return false;

            lock (resources)
            {
                if (resources.Count == 0 && string.IsNullOrEmpty(jid.Resource)) return true;
			    return resources.Exists(r => { return string.Compare(r, jid.Resource, StringComparison.OrdinalIgnoreCase) == 0; });
            }
		}

		public void BindResource(string resource)
		{
            lock (resources)
            {
                resources.Add(Stringprep.ResourcePrep(resource));
            }
			Connected = true;
		}

		public void UnbindResource(string resource)
		{
            lock (resources)
            {
                resources.Remove(Stringprep.ResourcePrep(resource));
            }
			Connected = resources.Count != 0;
		}

		private string Nameprep(string value)
		{
			return string.IsNullOrEmpty(value) ? value : Stringprep.NamePrep(value);
		}
	}
}