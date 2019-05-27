/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
