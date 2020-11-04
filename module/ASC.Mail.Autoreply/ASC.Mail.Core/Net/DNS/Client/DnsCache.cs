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


namespace ASC.Mail.Net.Dns.Client
{
    #region usings

    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    #endregion

    #region struct CacheEntry

    /// <summary>
    /// Dns cache entry.
    /// </summary>
    [Serializable]
    internal struct DnsCacheEntry
    {
        #region Members

        private readonly DnsServerResponse m_pResponse;
        private readonly DateTime m_Time;

        #endregion

        #region Properties

        /// <summary>
        /// Gets dns answers.
        /// </summary>
        public DnsServerResponse Answers
        {
            get { return m_pResponse; }
        }

        /// <summary>
        /// Gets entry add time.
        /// </summary>
        public DateTime Time
        {
            get { return m_Time; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="answers">Dns answers.</param>
        /// <param name="addTime">Entry add time.</param>
        public DnsCacheEntry(DnsServerResponse answers, DateTime addTime)
        {
            m_pResponse = answers;
            m_Time = addTime;
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// This class implements dns query cache.
    /// </summary>
    public class DnsCache
    {
        #region Members

        private static long m_CacheTime = 10000;
        private static Hashtable m_pCache;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets how long(seconds) to cache dns query.
        /// </summary>
        public static long CacheTime
        {
            get { return m_CacheTime; }

            set { m_CacheTime = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        static DnsCache()
        {
            m_pCache = new Hashtable();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to get dns records from cache, if any.
        /// </summary>
        /// <param name="qname"></param>
        /// <param name="qtype"></param>
        /// <returns>Returns null if not in cache.</returns>
        public static DnsServerResponse GetFromCache(string qname, int qtype)
        {
            try
            {
                if (m_pCache.Contains(qname + qtype))
                {
                    DnsCacheEntry entry = (DnsCacheEntry) m_pCache[qname + qtype];

                    // If cache object isn't expired
                    if (entry.Time.AddSeconds(m_CacheTime) > DateTime.Now)
                    {
                        return entry.Answers;
                    }
                }
            }
            catch {}

            return null;
        }

        /// <summary>
        /// Adds dns records to cache. If old entry exists, it is replaced.
        /// </summary>
        /// <param name="qname"></param>
        /// <param name="qtype"></param>
        /// <param name="answers"></param>
        public static void AddToCache(string qname, int qtype, DnsServerResponse answers)
        {
            if (answers == null)
            {
                return;
            }

            try
            {
                lock (m_pCache)
                {
                    // Remove old cache entry, if any.
                    if (m_pCache.Contains(qname + qtype))
                    {
                        m_pCache.Remove(qname + qtype);
                    }
                    m_pCache.Add(qname + qtype, new DnsCacheEntry(answers, DateTime.Now));
                }
            }
            catch {}
        }

        /// <summary>
        /// Clears DNS cache.
        /// </summary>
        public static void ClearCache()
        {
            lock (m_pCache)
            {
                m_pCache.Clear();
            }
        }

        /// <summary>
        /// Serializes current cache.
        /// </summary>
        /// <returns>Return serialized cache.</returns>
        public static byte[] SerializeCache()
        {
            lock (m_pCache)
            {
                MemoryStream retVal = new MemoryStream();

                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(retVal, m_pCache);

                return retVal.ToArray();
            }
        }

        /// <summary>
        /// DeSerializes stored cache.
        /// </summary>
        /// <param name="cacheData">This value must be DnsCache.SerializeCache() method value.</param>
        public static void DeSerializeCache(byte[] cacheData)
        {
            lock (m_pCache)
            {
                MemoryStream retVal = new MemoryStream(cacheData);

                BinaryFormatter b = new BinaryFormatter();
                m_pCache = (Hashtable) b.Deserialize(retVal);
            }
        }

        #endregion
    }
}