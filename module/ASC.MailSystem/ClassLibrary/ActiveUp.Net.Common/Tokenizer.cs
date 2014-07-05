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
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;

namespace ActiveUp.Net.Mail
{
    public class Tokenizer
    {
#if !PocketPC
        public static readonly string NewLine = Environment.NewLine;
#else
        public static readonly string NewLine = "\r\n";
#endif
        /// <summary>
        /// Tokenizes a string
        /// </summary>
        public static string[] Parse(string source)
        {
            string clean = CleanInput(source);
            string[] tokens = clean.Split(' ');
            ArrayList final = new ArrayList();
            foreach (string t in tokens)
            {
                string tmpt = t.ToLower();
                if (!final.Contains(tmpt))
                    final.Add(tmpt);
            }

            return (string[])final.ToArray(typeof(string));
        }

        public static void LoadFromFile(string fileName, ref Hashtable table)
        {           

            StreamReader Reader = new StreamReader(fileName);

            string res = Reader.ReadToEnd();

            //tokenize the entry
            string[] tokens = Tokenizer.Parse(res);

            Reader.Close();
         
            //log text
            foreach (string b in tokens)
            {
                if (table.ContainsKey(b))
                    table[b] = 1f + (float)table[b];
                else
                    table.Add(b, 1f);

            }

        }

        public static void AddWords(string fileName, string[] Words)
        {
            Hashtable tmp = new Hashtable();

            LoadFromFile(fileName, ref tmp);

            TeachListFile(fileName, Words, tmp);         
        }

        public static void TeachListFile(string fileName, string[] msgTokens, Hashtable currentHash)
        {
            // Wirte New Tokens
            StreamWriter W = new StreamWriter(fileName, true);
                       
            foreach (string t in msgTokens)
            {
                if (!currentHash.ContainsKey(t))
                    W.Write(t + " ");                
            }

            W.Close();
          
        }

        /// <summary>
        /// Replace invalid characters with spaces.
        /// </summary>
        static string CleanInput(string strIn)
        {
            return Regex.Replace(strIn, @"[^\w\'@-]", " ");
        }
    }
}
