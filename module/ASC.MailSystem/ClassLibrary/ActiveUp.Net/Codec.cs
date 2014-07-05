using System;

using System.Collections.Specialized;
using System.Collections.Generic;
namespace ActiveUp.Net
{
	/// <summary>
	/// Contains several static methods providing encoding/decoding in various formats.
	/// </summary>
	public abstract class Codec
	{
		public static string GetUniqueString()
		{
			return System.Diagnostics.Process.GetCurrentProcess().Id.ToString()+System.DateTime.Now.ToString("yyMMddhhmmss")+System.DateTime.Now.Millisecond.ToString()+(new Random().GetHashCode());
		}
		/// <summary>
		/// Encodes the text in quoted-printable format conforming to the RFC 2045 and RFC 2046.
		/// </summary>
		/// <param name="fromCharset">The charset of input.</param>
		/// <param name="input">Data to be encoded.</param>
		/// <remarks>The lines are wrapped to a length of max 78 characters to ensure data integrity across some gateways.</remarks>
		/// <returns>The input encoded as 7bit quoted-printable data, in the form of max 78 characters lines.</returns>
		/// <example>
		/// The example below illustrates the encoding of a string in quoted-printable.
		/// <code>
		/// C#
		/// 
		/// string input = "ActiveMail rocks ! Here are some non-ASCII characters =ç.";
		/// string output = Codec.ToQuotedPrintable(input,"iso-8859-1");
		/// </code>
		/// output returns A= ctiveMail rocks ! Here are some weird characters =3D=E7.
		/// 
		/// Non ASCII characters have been encoded (=3D represents = and =E7 represents ç).
		/// </example>
		public static string ToQuotedPrintable(string input, string fromCharset)

		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			byte[] body = System.Text.Encoding.GetEncoding(fromCharset).GetBytes(input);
			
			int index = 0, wrap = 0, check = 0;
			
			byte decim = 0;

			for(index=0;index<body.Length;index++)
			{
				if (wrap==0 && index+73-check < body.Length)
				{
					// it's a new line. Let's check if there is a bot in the next line.
					while(body[index+73-check] == 46)
					{
						wrap++;
						check++;
						if (wrap == 72 || index+73-check >= body.Length)
							break;
					}
					check=0;
				}
				
				decim = body[index];	
				if((decim<33 || decim==61 || decim>126) && decim!=32)
					sb.Append("="+decim.ToString("X").PadLeft(2,'0'));
				else
					sb.Append((char)decim);

				if (wrap >= 72)
				{
					sb.Append("=\r\n");
					wrap=0;
				}
				else
					wrap++;
				
				//if((sb.Length/72d)==System.Math.Round(sb.Length/72d) || ((sb.Length-1)/72d)==System.Math.Round((sb.Length-1)/72d) || ((sb.Length-2)/72d)==System.Math.Round((sb.Length-2)/72d) || ((sb.Length-3)/72d)==System.Math.Round((sb.Length-3)/72d))
			}
			//sb.Append("=\r\n");
			return sb.ToString();
		}
		/// <summary>
		/// Encodes the given string in a format (specified in RFC 2047) that can be used in RFC 2822 headers to represent non-ASCII textual data.
		/// </summary>
		/// <param name="input">The string to be encoded (the Header field's value).</param>
		/// <param name="charset">The charset of the Header field's value.</param>
		/// <returns>The encoded string with only 7bit characters.</returns>
		/// <remarks>ActiveMail only encodes in this format using Base64, but the RFC2047Decode method also decodes string encoded in this format with quoted-printable.</remarks>
		/// <example>
		/// The example below illustrates the encoding of a string.
		/// <code>
		/// C#
		/// 
		/// string input = "ActiveMail rocks ! Here are some non-ASCII characters =ç.";
		/// string output = Codec.RFC2047Encode(input,"iso-8859-1");
		/// </code>
		/// 
		/// output returns =?iso-8859-1?B?QWN0aXZlTWFpbCByb2NrcyAhIEhlcmUgYXJlIHNvbWUgd2VpcmQgY2hhcmFjdGVycyA95y4=?=
		/// 
		/// This value can be used as for example the subject of a message.
		/// If you suspect the text to contain non ASCII characters, do message.Subject = Codec.RFC2047Encode(yourRawValue);.
		/// </example>
		public static string RFC2047Encode(string input, string charset)
		{
			return "=?"+charset+"?B?"+System.Convert.ToBase64String(System.Text.Encoding.GetEncoding(charset).GetBytes(input))+"?=";
		}
		/// <summary>
		/// Decodes the given string from the format specified in RFC 2047 (=?charset?value?=).
		/// </summary>
		/// <param name="input">The string to be decoded.</param>
		/// <returns>The decoded string.</returns>
		/// <example>
		/// The example below illustrates the decoding of a string.
		/// <code>
		/// C#
		/// 
		/// string input = "I once wrote that =?iso-8859-1?B?QWN0aXZlTWFpbCByb2NrcyAhIEhlcmUgYXJlIHNvbWUgd2VpcmQgY2hhcmFjdGVycyA95y4=?=";
		/// string output = Codec.RFC2047Decode(input);
		/// </code>
		/// 
		/// output returns I once wrote that ActiveMail rocks ! Here are some weird characters =ç.
		/// </example>
		public static string RFC2047Decode(string input)
		{
			input = input.Replace("=?=","²rep?=");
			string decoded = input;
			if(input.IndexOf("=?")!=-1 && input.IndexOf("?=")!=-1)
			{
				string[] encodeds = System.Text.RegularExpressions.Regex.Split(input,System.Text.RegularExpressions.Regex.Escape("=?"));
				for(int i=1;i<encodeds.Length;i++)
				{
					string encoded = encodeds[i].Substring(0,encodeds[i].LastIndexOf("?="));
					string[] parts = encoded.Split('?');
					if(parts[1].ToUpper()=="Q") decoded = decoded.Replace("=?"+encoded+"?=",Codec.FromQuotedPrintable(System.Text.Encoding.GetEncoding(parts[0]).GetString(System.Text.Encoding.ASCII.GetBytes(parts[2].Replace("²rep","="))),parts[0]));
					else decoded = decoded.Replace("=?"+encoded+"?=",System.Text.Encoding.GetEncoding(parts[0]).GetString(System.Convert.FromBase64String(parts[2].Replace("²rep","="))));
				}
				decoded = decoded.Replace("_"," ");
			}
			else decoded = input;
			return decoded;
		}
		/// <summary>
		/// Decodes text from quoted-printable format defined in RFC 2045 and RFC 2046.
		/// </summary>
		/// <param name="toCharset">The original charset of input.</param>
		/// <param name="input">Data to be decoded.</param>
		/// <returns>The decoded data.</returns>
		/// <example>
		/// The example below illustrates the decoding of a string from quoted-printable.
		/// <code>
		/// C#
		/// 
		/// string input = "A=\r\nctiveMail rocks ! Here are some weird characters =3D=E7.";
		/// string output = Codec.FromQuotedPrintable(input,"iso-8859-1");
		/// </code>
		/// 
		/// output returns ActiveMail rocks ! Here are some weird characters =ç.
		/// </example>
		public static string FromQuotedPrintable(string input, string toCharset)
		{
			try
			{
				input = input.Replace("=\r\n","")+"=3D=3D";
				System.Collections.ArrayList arr = new System.Collections.ArrayList();
				int i=0;
				byte[] decoded = new byte[0];
				while(true)
				{
					if(i<=(input.Length)-3)
					{
						if(input[i]=='=' && input[i+1]!='=')
						{
							arr.Add(System.Convert.ToByte(System.Int32.Parse(String.Concat((char)input[i+1],(char)input[i+2]),System.Globalization.NumberStyles.HexNumber)));
							i += 3;
						}
						else
						{
							arr.Add((byte)input[i]);
							i++;
						}
					}
					else break;
				}
				decoded = new byte[arr.Count];
				for(int j=0;j<arr.Count;j++) decoded[j] = (byte)arr[j];
				return System.Text.Encoding.GetEncoding(toCharset).GetString(decoded).TrimEnd('=');
			}
			catch { return input; }
		}
        public static string GetFieldName(string input)
        {
            switch (input)
            {
                case "content-id": return "Content-ID";
                case "message-id": return "Message-ID";
                case "content-md5": return "Content-HexMD5Digest";
                case "mime-version": return "MIME-Version";
                default: return Codec.Capitalize(input);
            }
        }
        internal static string Capitalize(string input)
        {
            string output = string.Empty;
            string[] parts = input.Split('-');
            foreach (string str in parts) output += str[0].ToString().ToUpper() + str.Substring(1) + "-";
            return output.TrimEnd('-');
        }
		/// <summary>
		/// Wraps the given string to a set of lines of a maximum given length.
		/// </summary>
		/// <param name="input">Data to be wrapped.</param>
		/// <param name="totalchars">The maximum length for each line.</param>
		/// <returns>The data as a set of lines of a maximum length.</returns>
		/// <remarks>This can be used to wrap lines to a maximum length of 78 characters to ensure data integrity across some gateways.</remarks>
		public static string Wrap(string input, int totalchars)
		{
			totalchars -= 3;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			int i=0;
			for(i=0;(i+totalchars)<input.Length;i+=totalchars) sb.Append("\r\n"+input.Substring(i,totalchars));
			return (sb.ToString()+"\r\n"+input.Substring(i,input.Length-i)).TrimStart(new char[] {'\r','\n'});
		}
        public static string GetCRCBase64(string base64input)
        {
            byte[] binput = Convert.FromBase64String(base64input);
            const long CRC24_INIT = 0xb704ceL;
            const long CRC24_POLY = 0x1864cfbL;
            long crc = CRC24_INIT;
            for (int i = 0; i < binput.Length; i++)
            {
                crc ^= (((long)binput[i]) << 16);
                for (int j = 0; j < 8; j++)
                {
                    crc <<= 1;
                    if ((crc & 0x1000000) == 0x1000000)
                        crc ^= CRC24_POLY;
                }
            }
            byte a = (byte)(crc >> 16);
            byte b = (byte)((crc & 65280) >> 8);
            byte c = (byte)(crc & 255);
            byte[] d = { a, b, c };
            return Convert.ToBase64String(d);
        }
        public static string GetCRCBase64(byte[] input)
        {
            return GetCRCBase64(Convert.ToBase64String(input));
        }
        public static string ToRadix64(byte[] input)
        {
            return Convert.ToBase64String(input) + "\r\n=" + GetCRCBase64(input);
        }
        public static byte[] FromRadix64(string input)
        {
            string radix64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            input = input.TrimEnd('=');
            int length = input.Length - (input.Length % 4);
            byte[] inbytes = new byte[input.Length];
            for (int j = 0; j < inbytes.Length; j++)
            {
                inbytes[j] = (byte)radix64Alphabet.IndexOf(input[j]);
            }
            List<Byte> outbytes = new List<Byte>();
            for (int i = 0; i < length / 4; i++)
            {
                outbytes.Add(((byte)((inbytes[i * 4] << 2) + (inbytes[i * 4 + 1] >> 4))));
                outbytes.Add(((byte)(((inbytes[i * 4 + 1] & 15) << 4) + (inbytes[i * 4 + 2] >> 2))));
                outbytes.Add(((byte)(((inbytes[i * 4 + 2] & 3) << 6) + inbytes[i * 4 + 3])));
            }
            if ((input.Length % 4) == 3)
            {
                outbytes.Add(((byte)((inbytes[inbytes.Length - 3] << 2) + (inbytes[inbytes.Length - 3 + 1] >> 4))));
                outbytes.Add(((byte)(((inbytes[inbytes.Length - 3 + 1] & 15) << 4) + (inbytes[inbytes.Length - 3 + 2] >> 2))));
            }
            if ((input.Length % 4) == 2)
            {
                outbytes.Add(((byte)((inbytes[inbytes.Length - 2] << 2) + (inbytes[inbytes.Length - 2 + 1] >> 4))));
            }

            byte[] result = new byte[outbytes.Count];
            outbytes.CopyTo(result);
            return result;
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
        public static string ToBitString(short input)
        {
            string output = string.Empty;
            output += ((input & 32768) == 32768) ? "1" : "0";
            output += ((input & 16384) == 16384) ? "1" : "0";
            output += ((input & 8192) == 8192) ? "1" : "0";
            output += ((input & 4096) == 4096) ? "1" : "0";
            output += ((input & 2048) == 2048) ? "1" : "0";
            output += ((input & 1024) == 1024) ? "1" : "0";
            output += ((input & 512) == 512) ? "1" : "0";
            output += ((input & 256) == 256) ? "1" : "0";
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
        internal static byte FromBitString(string input)
        {
            byte output = 0;
            if (input[7].Equals('1')) output++;
            if (input[6].Equals('1')) output += 2;
            if (input[5].Equals('1')) output += 4;
            if (input[4].Equals('1')) output += 8;
            if (input[3].Equals('1')) output += 16;
            if (input[2].Equals('1')) output += 32;
            if (input[1].Equals('1')) output += 64;
            if (input[0].Equals('1')) output += 128;
            return output;
        }
    }
}