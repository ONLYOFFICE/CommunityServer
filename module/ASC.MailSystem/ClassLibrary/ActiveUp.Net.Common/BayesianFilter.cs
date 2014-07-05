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
#if !PocketPC
using System.Net.Mail;
#endif
using System.Collections.Specialized;
using System.Collections;

namespace ActiveUp.Net.Mail
{
    public class BayesianFilter
    {
        /// <summary>
        /// Analyzes a message if it is or not SPAM.
        /// Returns a boolean value for the validation.
        /// </summary>
        /// <param name="subject">The message subject.</param>
        /// <param name="body">The mail message body.</param>
        /// <param name="spamWordsFilename">The Spam Word List File.</param>
        /// <param name="hamWordsFilename">The Ham Word List File.</param>
        /// <param name="ignoreWordsFilename">The Ignore Word List File.</param>
        /// <returns>True for SPAM, false if it isn't a SPAM.</returns>
        static public bool AnalyzeMessage(string subject, string body, string spamWordsFilename, string hamWordsFilename, string ignoreWordsFilename)
        {
            // Load Spam Word List File
            Hashtable SpamTab = new Hashtable();
            Tokenizer.LoadFromFile(spamWordsFilename, ref SpamTab);

            // Load Ham Word List File
            Hashtable HamTab = new Hashtable();
            Tokenizer.LoadFromFile(hamWordsFilename, ref HamTab);

            // Load Ignore Word List File
            Hashtable IgnoreTab = new Hashtable();
            Tokenizer.LoadFromFile(ignoreWordsFilename, ref IgnoreTab);

            //Parse Message Into Tokens
            string[] msgTokens = Tokenizer.Parse(subject + " " + body);

            float I = 0;
            float invI = 0;

            foreach (string t in msgTokens)
            {
                if (!IgnoreTab.Contains(t))
                {
                    float SpamCount = SpamTab.ContainsKey(t) ? (float)SpamTab[t] : 0f;
                    float HamCount = HamTab.ContainsKey(t) ? (float)HamTab[t] : 0f;

                    if (SpamCount == 0 && HamCount == 0)
                        continue;

                    // Calculate Probability

                    float bw = SpamCount / SpamTab.Count;
                    float gw = HamCount / HamTab.Count;

                    float pw = ((bw) / ((bw) + (gw)));
                    float s = 1f, x = .5f, n = SpamCount + HamCount;
                    float fw = ((s * x) + (n * pw)) / (s + n);

                    // Log Probability
                    I = I == 0 ? fw : I * fw;
                    invI = invI == 0 ? (1 - fw) : invI * (1 - fw);
                }
                              
            }

            //Calculate Prediction

            float prediction = I / (I + invI);

            if (prediction <= .45)
            {
                // No Spam
                // Teach the Ham file based on the prediction
                //Tokenizer.TeachListFile(hamWordsFilename, msgTokens, HamTab);
                return false; 
            }
            else
                if (prediction >= .55)
                {
                    // Spam
                    // Teach the Spam file based on the prediction
                    //Tokenizer.TeachListFile(spamWordsFilename, msgTokens, SpamTab);
                    return true;
                }
            
            // prediction > .45 && prediction < .55 - Unable to determine - by default no SPAM
            
            // Teach the Ham file based on the prediction
            //Tokenizer.TeachListFile(hamWordsFilename, msgTokens, HamTab);
            
            return false;      

        }

        /// <summary>
        /// Reports a mail message as SPAM.
        /// It will be used for handle the AnalyzeMessage and
        /// identify if a message is or not a SPAM.
        /// </summary>
        /// <param name="message">The mail message.</param>
        /// <param name="filename">The file name for save it.</param>
        static public void ReportMessage(Message message, string filename)
        {
            string body = string.Empty;

            if (string.IsNullOrEmpty(message.BodyHtml.Text))
                body += message.BodyHtml.TextStripped;

            if (string.IsNullOrEmpty(message.BodyText.Text))
                body += message.BodyText.Text;

            string subject = Codec.RFC2047Decode(message.Subject);

            string[] tokens = Tokenizer.Parse(subject + " " + body);

            Tokenizer.AddWords(filename, tokens);
        }
    }
}
