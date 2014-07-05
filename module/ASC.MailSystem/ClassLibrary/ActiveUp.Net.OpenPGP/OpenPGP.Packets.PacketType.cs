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

namespace ActiveUp.Net.Security.OpenPGP.Packets
{
    /// <summary>
    /// PacketType Enum
    /// </summary>
    public enum PacketType
    {
        PublicKeyEncryptedSessionKey = 1,
        Signature = 2,
        SymmetricKeyEncryptedSessionKey = 3,
        OnePassSignature = 4,
        SecretKey = 5,
        PublicKey = 6,
        SecretSubkey = 7,
        Compressed = 8,
        SymmetricallyEncrypted = 9,
        Marker = 10,
        Literal = 11,
        Trust = 12,
        UserID = 13,
        PublicSubkey = 14,
        UserAttribute = 17,
        SymetricallyEncryptedAndIntegrityProtected = 18,
        ModificationDetectionCode = 19,
        Other = 0
    }
}
