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

using ASC.Xmpp.Core.utils.Xml.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Xmpp.Server.Gateway
{
	public interface IXmppConnection
	{
		string Id
		{
			get;
		}

		void Reset();

		void Close();

		void Send(Node node, Encoding encoding);

		void Send(string text, Encoding encoding);

		void BeginReceive();

		event EventHandler<XmppStreamStartEventArgs> XmppStreamStart;

		event EventHandler<XmppStreamEventArgs> XmppStreamElement;

		event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd;

		event EventHandler<XmppConnectionCloseEventArgs> Closed;
	}

	public class XmppStreamEventArgs : EventArgs
	{
		public string ConnectionId
		{
			get;
			private set;
		}

		public Node Node
		{
			get;
			private set;
		}

		public XmppStreamEventArgs(string connectionId, Node node)
		{
			if (string.IsNullOrEmpty(connectionId)) throw new ArgumentNullException("connectionId");
			if (node == null) throw new ArgumentNullException("node");

			ConnectionId = connectionId;
			Node = node;
		}
	}

	public class XmppStreamStartEventArgs : XmppStreamEventArgs
	{
		public string Namespace
		{
			get;
			private set;
		}

		public XmppStreamStartEventArgs(string connectionId, Node node, string ns)
			: base(connectionId, node)
		{
			Namespace = ns;
		}
	}

	public class XmppStreamEndEventArgs : EventArgs
	{
		public string ConnectionId
		{
			get;
			private set;
		}

		public ICollection<Node> NotSendedBuffer
		{
			get;
			private set;
		}

		public XmppStreamEndEventArgs(string connectionId, IEnumerable<Node> notSendedBuffer)
		{
            
			if (string.IsNullOrEmpty(connectionId)) throw new ArgumentNullException("connectionId");
		    
			ConnectionId = connectionId;
			if (notSendedBuffer == null)
			{
				NotSendedBuffer = new List<Node>();
			}
			else
			{
				NotSendedBuffer = new List<Node>(notSendedBuffer);
			}
		}
	}

	public class XmppConnectionCloseEventArgs : EventArgs
	{

	}
}
