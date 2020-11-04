/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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