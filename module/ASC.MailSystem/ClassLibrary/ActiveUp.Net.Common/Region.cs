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
using System.IO;
using System.Text.RegularExpressions;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Represents the dynamic region portion of the message.
    /// </summary>
    #if !PocketPC
    [System.Serializable]
    #endif
    public class Region
    {
        private string _regionID, _url, _content, _nulltext;
        private bool _Loaded;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Region()
        {
            this.RegionID = string.Empty;
            this.URL = string.Empty;
            this.NullText = string.Empty;
        }

        /// <summary>
        /// Creates the condition based on it's region id, field equal to value set with case-sensitive on.
        /// </summary>
        /// <param name="regionid">The ID of the region.</param>
        /// <param name="url">The url to retrieve.</param>
        public Region(string regionid, string url)
        {
            this.RegionID = regionid;
            this.URL = url;
            this.NullText = string.Empty;
        }

        public string NullText
        {
            get
            {
                return _nulltext;
            }
            set
            {
                _nulltext = value;
            }
        }
        /// <summary>
        /// The content to retrieve.
        /// </summary>
        public string Content
        {
            get
            {
                if (!_Loaded) 
                {
                    if (_url.Length>0)
                    {
                        _content = LoadFileContent(_url);
                        if (_content.Length==0)
                        {
                            _content = _nulltext;
                        }
                        _Loaded = true;
                    }
                    else
                    {
                        _content = _nulltext;
                    }
                }
                return _content;
            }
        }

        /// <summary>
        /// The ID of the region.
        /// </summary>
        public string RegionID
        {
            get
            {
                return _regionID;
            }
            set
            {
                _regionID = value;
            }
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string URL
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }
        
        /// <summary>
        /// Load the content of the specified file.
        /// </summary>
        /// <param name="filename">The full path to the file</param>
        /// <returns>The content of the file</returns>
        public string LoadFileContent(string filename)
        {
            string content = string.Empty;
            
                if (filename.ToUpper().StartsWith("HTTP://") || filename.ToUpper().StartsWith("HTTPS://"))
                {
                    System.IO.Stream stream;
                    System.Net.WebRequest webRequest;
                    System.Net.WebResponse webResponse;

                    webRequest = System.Net.WebRequest.Create(filename);
                    try 
                    {
                        webResponse = webRequest.GetResponse();
                        stream = webResponse.GetResponseStream();
                        content = new StreamReader(stream).ReadToEnd();
                        System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(@"<body.*?>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
                        System.Text.RegularExpressions.MatchCollection mc;
                        mc = rx.Matches(content);
                        if (mc.Count > 0)
                            {
                                foreach (System.Text.RegularExpressions.Match m in mc)
                                {
                                    content = m.Groups[1].Captures[0].Value;
                                }
                            }
                        } 
                        catch {
                            content = "";
                        }
                        }
                        else
                        {
                            if (filename.ToUpper().StartsWith("FILE://"))
                                filename = filename.Substring(7);

                            if (System.IO.File.Exists(filename))
                            {
                                TextReader textFileReader = TextReader.Synchronized(new StreamReader(filename, System.Text.UTF7Encoding.Default));
                                content = textFileReader.ReadToEnd();
                                textFileReader.Close();
                            }
                        else
                        {
                            content = "";
                        }
                    }
                    return content;
                }
        }
    }