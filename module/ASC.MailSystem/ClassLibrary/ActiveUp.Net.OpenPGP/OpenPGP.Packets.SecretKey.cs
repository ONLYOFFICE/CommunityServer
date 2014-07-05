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
using System.IO;
using ActiveUp.Net.OpenPGP;

namespace ActiveUp.Net.Security.OpenPGP.Packets
{
    /// <summary>
    /// SecretKey Class
    /// </summary>
    public class SecretKey : PublicKey
    {
        public new static SecretKey Parse(Packet input)
        {
            SecretKey sk = (SecretKey)input;

            sk = (SecretKey)PublicKey.Parse(input);

            MemoryStream ms = new MemoryStream(sk.RawData);
            ms.Position = sk.TempPosition;
            // String to key usage ?
            byte s2kUsage = (byte)ms.ReadByte();

            if (s2kUsage == 255 || s2kUsage == 254)
            {
                // We'll be using String to key...but with which algorithm ?
                sk.SymmetricKeyAlgorithm = (SymmetricKeyAlgorithm)ms.ReadByte();
                // And which type of S2K ?
                sk.StringToKeySpecifierType = (StringToKeySpecifierType)ms.ReadByte();
                sk.InitialVector = new byte[Constants.GetCipherBlockSize(sk.SymmetricKeyAlgorithm)];
                ms.Read(sk.InitialVector, 0, sk.InitialVector.Length);
                
            }
            else if (s2kUsage == 0)
            {

            }
            else
            {
                sk.SymmetricKeyAlgorithm = (SymmetricKeyAlgorithm)s2kUsage;
                sk.InitialVector = new byte[Constants.GetCipherBlockSize(sk.SymmetricKeyAlgorithm)];
                ms.Read(sk.InitialVector, 0, sk.InitialVector.Length);
            }

            // MPIs
            int mpiMaxCount = Constants.GetTotalMPICount(sk.PublicKeyAlgorithm);

            while (ms.Position < ms.Length && sk.MPIs.Count < mpiMaxCount)
            {
                byte first = (byte)ms.ReadByte();
                byte second = (byte)ms.ReadByte();
                short length = (short)((Converter.ToShort(new byte[2] { first, second }) + 7) / 8);
                byte[] mpi = new byte[(int)length];
                ms.Read(mpi, 0, length);
                sk.MPIs.Add(mpi);
            }

            sk.CheckSum = new byte[2];
            ms.Read(sk.CheckSum, 0, 2);
            
            return sk;
        }

        private SymmetricKeyAlgorithm _symmetricKeyAlgorithm;
        private StringToKeySpecifierType _s2kSpecifierType;
        private byte[] _iv;
        private byte[] _checkSum;

        public SymmetricKeyAlgorithm SymmetricKeyAlgorithm
        {
            get { return this._symmetricKeyAlgorithm; }
            set { this._symmetricKeyAlgorithm = value; }
        }
        public StringToKeySpecifierType StringToKeySpecifierType
        {
            get { return this._s2kSpecifierType; }
            set { this._s2kSpecifierType = value; }
        }
        public byte[] InitialVector
        {
            get { return this._iv; }
            set { this._iv = value; }
        }
        public byte[] CheckSum
        {
            get { return this._checkSum; }
            set { this._checkSum = value; }
        }
    }
}
