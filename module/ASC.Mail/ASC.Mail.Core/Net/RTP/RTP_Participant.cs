/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.RTP
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This is base class for <b>RTP_Participant_Local</b> and <b>RTP_Participant_Remote</b> class.
    /// </summary>
    public abstract class RTP_Participant
    {
        #region Events

        /// <summary>
        /// Is raised when participant disjoins(timeout or BYE all sources) the RTP multimedia session.
        /// </summary>
        public event EventHandler Removed = null;

        /// <summary>
        /// Is raised when participant gets new RTP source.
        /// </summary>
        public event EventHandler<RTP_SourceEventArgs> SourceAdded = null;

        /// <summary>
        /// Is raised when RTP source removed from(Timeout or BYE) participant.
        /// </summary>
        public event EventHandler<RTP_SourceEventArgs> SourceRemoved = null;

        #endregion

        #region Members

        private readonly string m_CNAME = "";
        private List<RTP_Source> m_pSources;

        #endregion

        #region Properties

        /// <summary>
        /// Gets canonical name of participant.
        /// </summary>
        public string CNAME
        {
            get { return m_CNAME; }
        }

        /// <summary>
        /// Gets the sources what participant owns(sends).
        /// </summary>
        public RTP_Source[] Sources
        {
            get { return m_pSources.ToArray(); }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="cname">Canonical name of participant.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>cname</b> is null reference.</exception>
        public RTP_Participant(string cname)
        {
            if (cname == null)
            {
                throw new ArgumentNullException("cname");
            }
            if (cname == string.Empty)
            {
                throw new ArgumentException("Argument 'cname' value must be specified.");
            }

            m_CNAME = cname;

            m_pSources = new List<RTP_Source>();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises <b>Removed</b> event.
        /// </summary>
        private void OnRemoved()
        {
            if (Removed != null)
            {
                Removed(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises <b>SourceAdded</b> event.
        /// </summary>
        /// <param name="source">RTP source.</param>
        private void OnSourceAdded(RTP_Source source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (SourceAdded != null)
            {
                SourceAdded(this, new RTP_SourceEventArgs(source));
            }
        }

        /// <summary>
        /// Raises <b>SourceRemoved</b> event.
        /// </summary>
        /// <param name="source">RTP source.</param>
        private void OnSourceRemoved(RTP_Source source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (SourceRemoved != null)
            {
                SourceRemoved(this, new RTP_SourceEventArgs(source));
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        internal void Dispose()
        {
            m_pSources = null;

            Removed = null;
            SourceAdded = null;
            SourceRemoved = null;
        }

        /// <summary>
        /// Adds specified source to participant if participant doesn't contain the specified source.
        /// </summary>
        /// <param name="source">RTP source.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>source</b> is null reference.</exception>
        internal void EnsureSource(RTP_Source source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (!m_pSources.Contains(source))
            {
                m_pSources.Add(source);

                OnSourceAdded(source);

                source.Disposing += delegate
                                        {
                                            if (m_pSources.Remove(source))
                                            {
                                                OnSourceRemoved(source);

                                                // If last source removed, the participant is dead, so dispose participant.
                                                if (m_pSources.Count == 0)
                                                {
                                                    OnRemoved();
                                                    Dispose();
                                                }
                                            }
                                        };
            }
        }

        #endregion
    }
}