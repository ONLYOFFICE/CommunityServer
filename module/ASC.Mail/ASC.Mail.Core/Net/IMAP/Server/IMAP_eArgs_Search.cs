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
