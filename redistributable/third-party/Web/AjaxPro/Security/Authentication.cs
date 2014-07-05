/*
using System;
using AjaxPro.Cryptography;

namespace AjaxPro.Security
{
	/// <summary>
	/// Represents error if ticket is too old.
	/// </summary>
	public class TicketExpiredException : Exception
	{
		private DateTime m_ExpireDate = DateTime.MinValue;

		public TicketExpiredException(DateTime expireDate)
		{
			m_ExpireDate = expireDate;
		}

		/// <summary>
		/// Returns the date the ticket was expired.
		/// </summary>
		public DateTime ExpireDate
		{
			get
			{
				return m_ExpireDate;
			}
		}

		public override string Message
		{
			get
			{
				return "Ticket has been expired!";
			}
		}

	}

	/// <summary>
	/// Represents a simple AuthenticationTicket class.
	/// </summary>
	public class Authentication : IAjaxAuthentication
	{
		private int m_UserID;
		private int m_DataSetID;
		private string m_DataSet;

		private DateTime m_TicketStartTime;
		private TimeSpan ExpireTimespan = new TimeSpan(24, 0, 0);

		/// <summary>
		/// 
		/// </summary>
		public Authentication()
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="UserID">The UserID stored in dbo.Accounts.</param>
		/// <param name="DataSet">The full dataset as a string.</param>
		/// <param name="DataSetID">The DataSetID stored in dbo.DataSets.</param>
		public Authentication(int UserID, int DataSetID, string DataSet)
		{
			m_UserID = UserID;
			m_DataSetID = DataSetID;
			m_DataSet = DataSet;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AuthenticationTicket">The encrypted ticket.</param>
		public Authentication(string AuthenticationTicket)
		{
			ParseTicket(AuthenticationTicket);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AuthenticationTicket">The encrypted ticket.</param>
		internal void ParseTicket(string AuthenticationTicket)
		{
			WebDecrypter webDec = new WebDecrypter();
			string plainTicket = webDec.Decrypt(AuthenticationTicket, "password");


			if(plainTicket.IndexOf("|") < 0)
				throw new Exception("Is not a valid AuthenticationTicket (1)!");

			string[] TicketItems = plainTicket.Split(new Char[]{'|'});

			if(TicketItems.Length != 4)
				throw new Exception("Is not a valid AuthenticationTicket (2)!");



			m_UserID = Convert.ToInt16(TicketItems[0]);
			m_DataSetID = Convert.ToInt16(TicketItems[1]);
			m_TicketStartTime = new DateTime(Convert.ToInt64(TicketItems[2]));
			m_DataSet = TicketItems[3];

			if(System.DateTime.Now - m_TicketStartTime > ExpireTimespan)
				throw new TicketExpiredException(m_TicketStartTime + ExpireTimespan);
		}

		/// <summary>
		/// Gets the AuthenticationTicket for accessing PC-Topp.NET Webservices.
		/// </summary>
		public string AuthenticationTicket
		{
			get
			{
				m_TicketStartTime = System.DateTime.Now;

				string plainTicket = m_UserID.ToString() + "|" + m_DataSetID.ToString() + "|" + m_TicketStartTime.Ticks + "|" + m_DataSet;

				WebEncrypter webEnc = new WebEncrypter();
				return webEnc.Encrypt(plainTicket, "password");
			}
		}

		/// <summary>
		/// Refresh an AuthenticationTicket.
		/// </summary>
		/// <param name="AuthenticationTicket">The encrypted ticket.</param>
		/// <returns>The refreshed encrypted ticket.</returns>
		public string RefreshTicket(string AuthenticationTicket)
		{
			ParseTicket(AuthenticationTicket);
			return this.AuthenticationTicket;
		}

		/// <summary>
		/// Gets the UserID.
		/// </summary>
		public int UserID
		{
			get
			{
				return m_UserID;
			}
			set
			{
				m_UserID = value;
			}
		}
	
		/// <summary>
		/// Gets the DataSetID for the specified DataSet.
		/// </summary>
		public int DataSetID
		{
			get
			{
				return m_DataSetID;
			}
			set
			{
				m_DataSetID = value;
			}
		}

		/// <summary>
		/// Gets the DataSet for the specified DataSet.
		/// </summary>
		public string DataSet
		{
			get
			{
				return m_DataSet;
			}
			set
			{
				m_DataSet = value;
			}
		}

		/// <summary>
		/// Gets the remaining time for this ticket after the ticket will be invalid.
		/// </summary>
		public TimeSpan RemainingTime
		{
			get
			{
				return ExpireTimespan - (System.DateTime.Now - m_TicketStartTime);
			}
		}
	}
}
*/