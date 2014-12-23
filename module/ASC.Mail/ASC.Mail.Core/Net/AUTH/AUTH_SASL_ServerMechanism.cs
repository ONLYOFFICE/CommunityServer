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

namespace ASC.Mail.Net.AUTH
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This base class for server SASL authentication mechanisms.
    /// </summary>
    public abstract class AUTH_SASL_ServerMechanism
    {
        #region Properties

        /// <summary>
        /// Gets if user has authenticated sucessfully.
        /// </summary>
        public abstract bool IsAuthenticated { get; }

        /// <summary>
        /// Gets if the authentication exchange has completed.
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// Gets IANA-registered SASL authentication mechanism name.
        /// </summary>
        /// <remarks>The registered list is available from: http://www.iana.org/assignments/sasl-mechanisms .</remarks>
        public abstract string Name { get; }

        /// <summary>
        /// Gets if specified SASL mechanism is available only to SSL connection.
        /// </summary>
        public abstract bool RequireSSL { get; }

        /// <summary>
        /// Gets user login name.
        /// </summary>
        public abstract string UserName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Continues authentication process.
        /// </summary>
        /// <param name="clientResponse">Client sent SASL response.</param>
        /// <returns>Retunrns challange response what must be sent to client or null if authentication has completed.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>clientRespone</b> is null reference.</exception>
        public abstract string Continue(string clientResponse);

        #endregion
    }
}