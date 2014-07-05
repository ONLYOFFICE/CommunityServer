/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Server.Utils;

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
			get { return resources.AsReadOnly(); }
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

			if (resources.Count ==0 && string.IsNullOrEmpty(jid.Resource)) return true;

			return resources.Exists(r => { return string.Compare(r, jid.Resource, StringComparison.OrdinalIgnoreCase) == 0; });
		}

		public void BindResource(string resource)
		{
			resources.Add(Stringprep.ResourcePrep(resource));
			Connected = true;
		}

		public void UnbindResource(string resource)
		{
			resources.Remove(Stringprep.ResourcePrep(resource));
			Connected = resources.Count != 0;
		}

		private string Nameprep(string value)
		{
			return string.IsNullOrEmpty(value) ? value : Stringprep.NamePrep(value);
		}
	}
}