// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Security.OpenPGP
{
    /// <summary>
    /// SignatureSubPacketType Enum
    /// </summary>
    public enum SignatureSubPacketType
    {
        CreationTime = 2,
        ExpirationTime = 3,
        ExportableCertification = 4,
        TrustSignature = 5,
        RegularExpression = 6,
        Revocable = 7,
        KeyExpirationTime = 9,
        PreferredSymmetricAlgorithms = 11,
        RevocationKey = 12,
        IssuerKeyID = 16,
        NotationData = 20,
        PreferredHashAlgorithms = 21,
        PreferredCompressionAlgorithms = 22,
        KeyServerPreferences = 23,
        PreferredKeyServer = 24,
        PrimaryUserID = 25,
        PolicyURL = 26,
        KeyFlags = 27,
        SignerUserID = 28,
        ReasonForRevocation = 29,
        Features = 30,
        SignatureTarget = 31,
        EmbeddedSignature = 32
    }
}