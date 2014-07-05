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

namespace ActiveUp.Net.Dns
{
    public enum ReturnCode
    {
        Success = 0,
        FormatError = 1,
        ServerFailure = 2,
        NameError = 3,
        NotImplemented = 4,
        Refused = 5,
        Other = 6
    }

    public class DnsAnswer
    {    
        /// <summary>
        /// Given a byte array  interpret them as a DnsEntry  
        /// </summary>
        /// <param name="response"></param>
        public DnsAnswer(byte[] response)
        {
            questions = new List<Question>();
            answers = new List<Answer>();
            servers = new List<Server>();
            additional = new List<Record>();
            exceptions = new List<Exception>();
            DataBuffer buffer = new DataBuffer(response, 2);
            byte bits1 = buffer.ReadByte();
            byte bits2 = buffer.ReadByte();
            //Mask off return code
            int returnCode = bits2 & 15;
            if (returnCode > 6)  returnCode = 6;
            this.returnCode = (ReturnCode)returnCode;
            //Get Additional Flags
            authoritative = TestBit(bits1, 2);
            recursive = TestBit(bits2, 8);
            truncated = TestBit(bits1, 1);

            int nQuestions  = buffer.ReadBEShortInt();
            int nAnswers    = buffer.ReadBEShortInt();
            int nServers    = buffer.ReadBEShortInt();
            int nAdditional = buffer.ReadBEShortInt();
            
            //read in questions
            for(int i = 0; i < nQuestions; i++)
            {
                try
                {
                    questions.Add(new Question(buffer));
                }
                catch (Exception ex)
                {                    
                    exceptions.Add(ex);
                }
            }
            //read in answers
            for(int i = 0; i < nAnswers; i++)
            {
                try
                {
                    answers.Add(new Answer(buffer));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            //read in servers
            for(int i = 0; i < nServers; i++)
            {
                try
                {
                    servers.Add(new Server(buffer));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            //read in additional records 
            for(int i = 0; i < nAdditional; i++)
            {
                try
                {
                    additional.Add(new Record(buffer));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }            
        }

        /// <summary>
        /// Test Bit position
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool TestBit(Byte b, byte pos)
        {
            byte mask = (byte)(0x01 << pos);
            return ((b & mask) != 0);
        }

        private ReturnCode returnCode = ReturnCode.Other;
        /// <summary>
        /// Access return code from server
        /// </summary>
        public ReturnCode ReturnCode
        {
            get { return returnCode; }
        }
        private bool authoritative;
        /// <summary>
        /// Was response authoritative
        /// </summary>
        public bool Authoritative
        {
            get { return authoritative; }
        }
        private bool recursive;
        /// <summary>
        /// Was answer recursive
        /// </summary>
        public bool Recursive
        {
            get { return recursive; }
        }
        private bool truncated;
        /// <summary>
        /// was answer truncated
        /// </summary>
        public bool Truncated
        {
            get { return truncated; }
        }
        private List<Question> questions;
        /// <summary>
        /// Access the list of questions
        /// </summary>
        public List<Question> Questions
        {
            get { return questions; }
        }
        private List<Answer> answers;
        /// <summary>
        /// Access list of answers
        /// </summary>
        public List<Answer> Answers
        {
            get { return answers; }
        }
        public List<Server> servers;
        /// <summary>
        /// Access list of servers
        /// </summary>
        public List<Server> Servers
        {
            get { return servers; }
        }
        private List<Record> additional;
        /// <summary>
        /// Access list of additional records
        /// </summary>
        public List<Record> Additional
        {
            get { return additional; }
        }
        private List<Exception> exceptions;
        /// <summary>
        /// access list of exceptions that were created during reading the response
        /// </summary>
        public List<Exception> Exceptions
        {
            get { return exceptions; }
        }

        /// <summary>
        /// Concatenate all the lists and return as one big list
        /// </summary>
        public List<DnsEntry> Entries
        {
            get
            {
                List<DnsEntry> res = new List<DnsEntry>();
                foreach (Answer ans in answers)
                    res.Add(ans);

                foreach (Server svr in servers)
                    res.Add(svr);

                foreach (Record adl in additional)
                    res.Add(adl);
                
                return res;
            }
        }

    }
}
