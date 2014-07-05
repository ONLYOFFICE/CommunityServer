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
using ActiveUp.Net.OpenPGP;

namespace ActiveUp.Net.Security.OpenPGP.Packets
{
    /// <summary>
    /// PublicKeyEncryptedSessionKey Class
    /// </summary>
    public class PublicKeyEncryptedSessionKey : Packet
    {
        // TODO : Clés spécifiques à l'algorithme

        public static PublicKeyEncryptedSessionKey Parse(Packet input)
        {
            PublicKeyEncryptedSessionKey pkesk = (PublicKeyEncryptedSessionKey)input;
            byte[] content = pkesk.RawData;
            pkesk.VersionNumber = content[0];
            
            byte[] keyID = new byte[8];
            Array.Copy(content, 1, keyID, 0, 8);
            //ActiveUp.Net.Mail.Logger.AddEntry(Encoding.ASCII.GetString(keyID));
            pkesk.KeyID = Converter.ToULong(keyID);

            pkesk.PublicKeyAlgorithm = (PublicKeyAlgorithm)content[9];

            return pkesk;
        }

        private int _versionNumber;
        private ulong _keyID;
        private PublicKeyAlgorithm _pkAlg;
        private byte[] _sessionKey;

        public int VersionNumber
        {
            get { return this._versionNumber; }
            set { this._versionNumber = value; }
        }
        public ulong KeyID
        {
            get { return this._keyID; }
            set { this._keyID = value; }
        }
        public PublicKeyAlgorithm PublicKeyAlgorithm
        {
            get { return this._pkAlg; }
            set { this._pkAlg = value; }
        }
        public byte[] SessionKey
        {
            get { return this._sessionKey; }
            set { this._sessionKey = value; }
        }
    }
}