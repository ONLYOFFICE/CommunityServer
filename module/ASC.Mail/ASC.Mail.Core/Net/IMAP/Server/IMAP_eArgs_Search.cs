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

namespace LumiSoft.Net.IMAP.Server
{
	/// <summary>
	/// Provides data from IMAP Search command.
	/// </summary>
	public class IMAP_eArgs_Search
	{
		private IMAP_Session       m_pSession  = null;
		private string             m_Folder    = "";
		private IMAP_Messages      m_pMessages = null;
		private IMAP_SearchMatcher m_pMatcher  = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="session">IMAP session what calls this search.</param>
		/// <param name="folder">IMAP folder name which messages to search.</param>
		/// <param name="matcher">Matcher what must be used to check if message matches searching criterial.</param>
		public IMAP_eArgs_Search(IMAP_Session session,string folder,IMAP_SearchMatcher matcher)
		{
			m_pSession  = session;
			m_Folder    = folder;
			m_pMatcher  = matcher;
			
			m_pMessages = new IMAP_Messages(folder);
		}

        
		#region Properties Implementation

		/// <summary>
		/// Gets current session.
		/// </summary>
		public IMAP_Session Session
		{
			get{ return m_pSession; }
		}

		/// <summary>
		/// Gets folder name which messages to search.
		/// </summary>
		public string Folder
		{
			get{ return m_Folder; }
		}

		/// <summary>
		/// Gets or sets messges what match searching criterial. 
		/// </summary>
		public IMAP_Messages MatchingMessages
		{
			get{ return m_pMessages; }
		}

		/// <summary>
		/// Gets matcher what must be used to check if message matches searching criterial.
		/// </summary>
		public IMAP_SearchMatcher Matcher
		{
			get{ return m_pMatcher; }
		}

		#endregion

	}
}
