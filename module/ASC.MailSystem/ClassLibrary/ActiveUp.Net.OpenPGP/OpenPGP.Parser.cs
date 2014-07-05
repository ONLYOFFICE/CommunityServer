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
using System.Text.RegularExpressions;
using ActiveUp.Net.Security.OpenPGP.Packets;

namespace ActiveUp.Net.Security.OpenPGP
{
    /// <summary>
    /// Parser Class
    /// </summary>
    public class Parser
    {
        public static ASCIIArmoredMessage ParseMessage(string input)
        {
            ASCIIArmoredMessage message = new ASCIIArmoredMessage();
            string messageType = Regex.Match(input, @"(?<=-----BEGIN PGP )[A-Z ]+(?=-----)").Value;
            if (messageType.Equals("MESSAGE")) message.Type = ASCIIArmoredMessageType.Message;
            else if (messageType.Equals("PUBLIC KEY BLOCK")) message.Type = ASCIIArmoredMessageType.PublicKeyBlock;
            else if (messageType.Equals("PRIVATE KEY BLOCK")) message.Type = ASCIIArmoredMessageType.PrivateKeyBlock;
            else if (messageType.Equals("MESSAGE, PART X/Y")) message.Type = ASCIIArmoredMessageType.MessagePartXOutOfY;
            else if (messageType.Equals("MESSAGE, PART X")) message.Type = ASCIIArmoredMessageType.MessagePartX;
            else if (messageType.Equals("SIGNATURE")) message.Type = ASCIIArmoredMessageType.Signature;

            MatchCollection matches = Regex.Matches(input, @"\S+:(.|(\r?\n[\t ]))+");
            foreach (Match m in matches)
            {
                string name = m.Value.Substring(0, m.Value.IndexOf(':')).ToLower().Trim('\r', '\n'); ;
                string value = m.Value.Substring(m.Value.IndexOf(':') + 1).Trim('\r', '\n').TrimEnd(' ','\t');
                message.Headers.Add(name, value);
            }

            input = input.Trim('\r','\n');
            Match start = Regex.Match(input,@"\r\n[ \t]*\r\n");
            string packet = input.Substring(start.Index + start.Length);

            string checksum = Regex.Match(packet, @"(?<=\r\n=)[A-Za-z0-9+/=]{4}(?=\r\n-----END)").Value;
            message.CRCBase64 = checksum;

            packet = packet.Substring(0,Regex.Match(packet,@"\r\n[A-Za-z0-9+/=]{5}\r\n-----END").Index);

            //ActiveUp.Net.Mail.Logger.AddEntry(checksum);

            byte[] packetb = Convert.FromBase64String(packet);
            
            MemoryStream packetms = new MemoryStream(packetb);

            while (packetms.Position < packetms.Length)
            {
                Packet packetObj = GetNextPacket(ref packetms);
                message.Packets.Add(DispatchPacket(packetObj));
            }
            packetms.Close();

#if !PocketPC
            packetms.Dispose();
#endif

            return message;
        }
        private static Packet DispatchPacket(Packet packet)
        {
            Type t = Type.GetType("ActiveUp.Net.Security.OpenPGP.Packets." + packet.Type.ToString());
            object pack = null;
#if !PocketPC
            pack = System.Activator.CreateInstance("ActiveUp.Mail", "ActiveUp.Net.Security.OpenPGP.Packets." + packet.Type.ToString()).Unwrap();
#else
            pack = System.Activator.CreateInstance(t);
#endif
            return (Packet)t.GetMethod("Parse").Invoke(pack, new object[] { packet });
        }
        private static Packet GetNextPacket(ref MemoryStream stream)
        {
            Packet packet = new Packet();

            byte firstbyte = (byte)stream.ReadByte();
            string first = ToBitString(firstbyte);
            if (!first[0].Equals('1')) throw new InvalidPacketSyntaxException("First byte's first bit is 0.");

            PacketFormat format = (PacketFormat)Int32.Parse(first[1].ToString());

            MemoryStream outstream = new MemoryStream();
            
            if (format.Equals(PacketFormat.Old))
            {
                PacketType type = (PacketType)((firstbyte & 60) >> 2);

#if !PocketPC
                packet = (Packet)(System.Activator.CreateInstance("ActiveUp.Mail", "ActiveUp.Net.Security.OpenPGP.Packets."+type.ToString()).Unwrap());
#else
                packet = (Packet)(System.Activator.CreateInstance(Type.GetType("ActiveUp.Net.Security.OpenPGP.Packets."+type.ToString())));
#endif

                //ActiveUp.Net.Mail.Logger.AddEntry("Packet : "+packet.ToString());

                packet.Format = format;
                packet.Type = type;

                byte lengthType = (byte)(firstbyte & 3);
                byte next = (byte)stream.ReadByte();
                if (lengthType == 0) 
                {
                    packet.BodyLength = next;
                    packet.TotalLength = packet.BodyLength + 2;
                }
                else if (lengthType == 1)
                {
                    byte nextnext = (byte)stream.ReadByte();
                    packet.BodyLength = (next << 8) + nextnext;
                    packet.TotalLength = packet.BodyLength + 3;
                }
                else if (lengthType == 2)
                {
                    // A VERIFIER !
                    byte nextnext = (byte)stream.ReadByte();
                    byte nextnextnext = (byte)stream.ReadByte();
                    byte nextnextnextnext = (byte)stream.ReadByte();
                    packet.BodyLength = (next << 24) + (nextnext << 16) + (nextnextnext << 8) + nextnextnextnext;
                    packet.TotalLength = packet.BodyLength + 5;
                }
                else if (lengthType == 3) packet.TotalLength = packet.BodyLength + 1;
                else throw new InvalidPacketSyntaxException("Invalid old format packet length type : "+lengthType.ToString());
                outstream.Write(stream.ToArray(), (int)stream.Position, packet.BodyLength);
                stream.Position += packet.BodyLength;
            }
            if (format.Equals(PacketFormat.New))
            {
                PacketType type = (PacketType)(firstbyte & 63);
                
                #if !PocketPC
                packet = (Packet)(System.Activator.CreateInstance("ActiveUp.Mail", "ActiveUp.Net.Security.OpenPGP.Packets." + type.ToString()).Unwrap());
                #else
                packet = (Packet)(System.Activator.CreateInstance(Type.GetType("ActiveUp.Net.Security.OpenPGP.Packets." + type.ToString())));
                #endif
                
                packet.Format = format;
                packet.Type = type;

                AddNextPacketNewFormat(ref packet, ref stream, ref outstream);
            }
            packet.RawData = outstream.ToArray();
            outstream.Close();

#if !PocketPC
            outstream.Dispose();
#endif
            return packet;
        }
        private static void AddNextPacketNewFormat(ref Packet packet, ref MemoryStream stream, ref MemoryStream outstream)
        {
            byte next = (byte)stream.ReadByte();
            int length = 0;
            if (next < 192)
            {
                length = next;
                packet.BodyLength += next;
                packet.TotalLength += packet.BodyLength + 2;
            }
            else if (next > 191 && next < 223)
            {
                byte nextnext = (byte)stream.ReadByte();
                length = ((next - 192) << 8) + nextnext + 192;
                packet.BodyLength += length;
                packet.TotalLength += packet.BodyLength + 3;
            }
            else if (next == 255)
            {
                int nextnext = stream.ReadByte();
                int nextnextnext = stream.ReadByte();
                int nextnextnextnext = stream.ReadByte();
                int nextnextnextnextnext = stream.ReadByte();
                length = (nextnext << 24) | (nextnextnext << 16) | (nextnextnextnext << 8) | nextnextnextnextnext;
                packet.BodyLength += length;
                packet.TotalLength += packet.BodyLength + 6;
            }
            else if (next > 223 && next < 255)
            {
                int partlength = (1 << (next & 31));
                packet.BodyLength += partlength;
                packet.TotalLength += partlength + 1;
                outstream.Write(stream.ToArray(), (int)stream.Position, partlength);
                stream.Position += partlength;
                AddNextPacketNewFormat(ref packet, ref stream, ref outstream);
                return;
            }
            outstream.Write(stream.ToArray(), (int)stream.Position, length);
            stream.Position += packet.BodyLength;
        }
        public static string ToBitString(byte input)
        {
            string output = string.Empty;
            output += ((input & 128) == 128) ? "1" : "0";
            output += ((input & 64) == 64) ? "1" : "0";
            output += ((input & 32) == 32) ? "1" : "0";
            output += ((input & 16) == 16) ? "1" : "0";
            output += ((input & 8) == 8) ? "1" : "0";
            output += ((input & 4) == 4) ? "1" : "0";
            output += ((input & 2) == 2) ? "1" : "0";
            output += ((input & 1) == 1) ? "1" : "0";
            return output;
        }
        
    }
}
