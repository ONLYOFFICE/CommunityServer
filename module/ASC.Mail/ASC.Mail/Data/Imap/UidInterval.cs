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


using System;

namespace ASC.Mail.Data.Imap
{
    public class UidInterval
    {
        public UidInterval(int from, int to)
        {
            if (from < 1)
                throw new ArgumentException("UidlInterval constructor from argument must be greater then 1");

            if (to < 1)
                throw new ArgumentException("UidlInterval constructor to argument must be greater then 1");

            if (from > to)
                throw new ArgumentException("UidlInterval constructor to argument must be greater then from argument");

            From = from;
            To = to;
        }

        public int From { get; private set; }
        public int To { get; private set; }

        public bool In(int val)
        {
            return val >= From && val <= To;
        }

        public static bool operator ==(UidInterval a, UidInterval b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.From == b.From && a.To == b.To;
        }

        public static bool operator !=(UidInterval a, UidInterval b)
        {
            return !(a == b);
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var p = obj as UidInterval;
            if ((Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (From == p.From) && (To == p.To);
        }

        public bool Equals(UidInterval p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (From == p.From) && (To == p.To);
        }

        public override int GetHashCode()
        {
            return From ^ To;
        }

        public bool IsToUidMax()
        {
            return To == int.MaxValue;
        }

        public bool IsFromUidMin()
        {
            return From == 1;
        }
    }
}
