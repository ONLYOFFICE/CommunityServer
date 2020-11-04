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


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    #endregion

    /// <summary>
    /// Common utility methods.
    /// </summary>
    public class Net_Utils
    {
        #region Methods

        /// <summary>
        /// Gets local host name or argument <b>hostName</b> value if it's specified.
        /// </summary>
        /// <param name="hostName">Host name or null.</param>
        /// <returns>Returns local host name or argument <b>hostName</b> value if it's specified.</returns>
        public static string GetLocalHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                return System.Net.Dns.GetHostName();
            }
            else
            {
                return hostName;
            }
        }

        /// <summary>
        /// Compares if specified array itmes equals.
        /// </summary>
        /// <param name="array1">Array 1.</param>
        /// <param name="array2">Array 2</param>
        /// <returns>Returns true if both arrays are equal.</returns>
        public static bool CompareArray(Array array1, Array array2)
        {
            return CompareArray(array1, array2, array2.Length);
        }

        /// <summary>
        /// Compares if specified array itmes equals.
        /// </summary>
        /// <param name="array1">Array 1.</param>
        /// <param name="array2">Array 2</param>
        /// <param name="array2Count">Number of bytes in array 2 used for compare.</param>
        /// <returns>Returns true if both arrays are equal.</returns>
        public static bool CompareArray(Array array1, Array array2, int array2Count)
        {
            if (array1 == null && array2 == null)
            {
                return true;
            }
            if (array1 == null && array2 != null)
            {
                return false;
            }
            if (array1 != null && array2 == null)
            {
                return false;
            }
            if (array1.Length != array2Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < array1.Length; i++)
                {
                    if (!array1.GetValue(i).Equals(array2.GetValue(i)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Copies <b>source</b> stream data to <b>target</b> stream.
        /// </summary>
        /// <param name="source">Source stream. Reading starts from stream current position.</param>
        /// <param name="target">Target stream. Writing starts from stream current position.</param>
        /// <param name="blockSize">Specifies transfer block size in bytes.</param>
        /// <returns>Returns number of bytes copied.</returns>
        public static long StreamCopy(Stream source, Stream target, int blockSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (blockSize < 1024)
            {
                throw new ArgumentException("Argument 'blockSize' value must be >= 1024.");
            }

            byte[] buffer = new byte[blockSize];
            long totalReaded = 0;
            while (true)
            {
                int readedCount = source.Read(buffer, 0, buffer.Length);
                // We reached end of stream, we readed all data sucessfully.
                if (readedCount == 0)
                {
                    return totalReaded;
                }
                else
                {
                    target.Write(buffer, 0, readedCount);
                    totalReaded += readedCount;
                }
            }
        }

        /// <summary>
        /// Gets if the specified string value is IP address.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified value is IP address.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public static bool IsIPAddress(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            IPAddress ip = null;

            return IPAddress.TryParse(value, out ip);
        }

        /// <summary>
        /// Gets if the specified IP address is multicast address.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <returns>Returns true if <b>ip</b> is muticast address, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> s null reference.</exception>
        public static bool IsMulticastAddress(IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentNullException("ip");
            }

            // IPv4 multicast 224.0.0.0 to 239.255.255.255

            if (ip.IsIPv6Multicast)
            {
                return true;
            }
            else if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] bytes = ip.GetAddressBytes();
                if (bytes[0] >= 224 && bytes[0] <= 239)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses IPEndPoint from the specified string value.
        /// </summary>
        /// <param name="value">IPEndPoint string value.</param>
        /// <returns>Returns parsed IPEndPoint.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static IPEndPoint ParseIPEndPoint(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            try
            {
                string[] ip_port = value.Split(':');

                return new IPEndPoint(IPAddress.Parse(ip_port[0]), Convert.ToInt32(ip_port[1]));
            }
            catch (Exception x)
            {
                throw new ArgumentException("Invalid IPEndPoint value.", "value", x);
            }
        }

        /// <summary>
        /// Gets if IO completion ports supported by OS.
        /// </summary>
        /// <returns></returns>
        public static bool IsIoCompletionPortsSupported()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.SetBuffer(new byte[0], 0, 0);
                e.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 111);
                s.SendToAsync(e);

                return true;
            }
            catch (NotSupportedException nX)
            {
                string dummy = nX.Message;

                return false;
            }
            finally
            {
                s.Close();
            }
        }

        /// <summary>
        /// Converts specified string to HEX string.
        /// </summary>
        /// <param name="text">String to convert.</param>
        /// <returns>Returns hex string.</returns> 
        public static string Hex(string text)
        {
            return BitConverter.ToString(Encoding.Default.GetBytes(text)).ToLower().Replace("-", "");
        }

        #endregion
    }
}