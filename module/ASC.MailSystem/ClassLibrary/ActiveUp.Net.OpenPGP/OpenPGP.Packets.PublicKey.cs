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
using System.IO;
using ActiveUp.Net.OpenPGP;

namespace ActiveUp.Net.Security.OpenPGP.Packets
{
    /// <summary>
    /// PublicKey Class
    /// </summary>
    public class PublicKey : Packet
    {
        public static PublicKey Parse(Packet input)
        {
            PublicKey pk = (PublicKey)input;

            MemoryStream ms = new MemoryStream(pk.RawData);

            pk.VersionNumber = ms.ReadByte();

            byte[] creation = new byte[4];
            ms.Read(creation, 0, 4);
            pk.CreationTime = Converter.UnixTimeStampToDateTime(Converter.ToInt(creation));

            if (pk.VersionNumber.Equals(3))
            {
                byte[] validity = new byte[2];
                ms.Read(validity, 0, 2);
                pk.Validity = (int)Converter.ToShort(validity);
            }

            pk.PublicKeyAlgorithm = (PublicKeyAlgorithm)ms.ReadByte();

            // MPIs
            int mpiMaxCount = Constants.GetPublicMPICount(pk.PublicKeyAlgorithm);
            
            while (ms.Position < ms.Length && pk.MPIs.Count < mpiMaxCount)
            {
                byte first = (byte)ms.ReadByte();
                byte second = (byte)ms.ReadByte();
                short length = (short)((Converter.ToShort(new byte[2] { first, second }) + 7) / 8);
                byte[] mpi = new byte[(int)length];
                ms.Read(mpi, 0, length);
                pk.MPIs.Add(mpi);
            }

            pk.TempPosition = (int)ms.Position;

            return pk;
        }

        private int _versionNumber, _validity;
        private PublicKeyAlgorithm _pkAlg;
        private DateTime _creation;
        private List<byte[]> _mpis = new List<byte[]>();


        public DateTime CreationTime
        {
            get { return this._creation; }
            set { this._creation = value; }
        }
        public int VersionNumber
        {
            get { return this._versionNumber; }
            set { this._versionNumber = value; }
        }
        public int Validity
        {
            get { return this._validity; }
            set { this._validity = value; }
        }
        public PublicKeyAlgorithm PublicKeyAlgorithm
        {
            get { return this._pkAlg; }
            set { this._pkAlg = value; }
        }
        public List<byte[]> MPIs
        {
            get { return this._mpis; }
            set { this._mpis = value; }
        }
    }
}
