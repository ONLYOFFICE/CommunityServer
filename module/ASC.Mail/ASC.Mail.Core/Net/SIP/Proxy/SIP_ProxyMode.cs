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

namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Specifies SIP proxy mode.
    /// <example>
    /// All flags may be combined, except Stateless,Statefull,B2BUA.
    /// For example: (Stateless | Statefull) not allowed, but (Registrar | Presence | Statefull) is allowed.
    /// </example>
    /// </summary>
    [Flags]
    public enum SIP_ProxyMode
    {
        /// <summary>
        /// Proxy implements SIP registrar.
        /// </summary>
        Registrar = 1,

        /// <summary>
        /// Proxy implements SIP presence server.
        /// </summary>
        Presence = 2,

        /// <summary>
        /// Proxy runs in stateless mode.
        /// </summary>
        Stateless = 4,

        /// <summary>
        /// Proxy runs in statefull mode.
        /// </summary>
        Statefull = 8,

        /// <summary>
        /// Proxy runs in B2BUA(back to back user agent) mode.
        /// </summary>
        B2BUA = 16,
    }
}