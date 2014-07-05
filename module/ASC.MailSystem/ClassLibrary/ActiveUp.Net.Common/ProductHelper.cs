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

namespace ActiveUp.Net.Mail
{
    internal class ProductHelper
    {
        /// <summary>
        /// Gets the trial string.
        /// </summary>
        /// <param name="stringToModify">The string to modify.</param>
        /// <param name="trialStringType">Type of the trial string.</param>
        /// <returns></returns>
        public static string GetTrialString(string stringToModify, TrialStringType trialStringType)
        {
            // Ensure that we have something to work with
            if (stringToModify == null) stringToModify = string.Empty;

            // Create the random texts
            Random random = new Random();

            string shortText = GetRandomString(9 + random.Next(5));
            string longText = string.Format(" (This email has been created using the trial version of ActiveUp.MailSystem. When you register the product, this message and all trial texts disappear. In addition of this text, the subject, the attachment filenames and the recipient/sender names are modified with a random trial string (ex:{0}). http://www.activeup.com) ", shortText);

            int randomPosition = random.Next(stringToModify.Length);

            // Apply the texts
            switch (trialStringType)
            {
                case TrialStringType.ShortText:
                default:
                    return stringToModify.Insert(randomPosition, shortText);
                case TrialStringType.LongHtml:
                    return stringToModify.Insert(randomPosition, string.Format("<table bgcolor=\"#F00000\" border=\"1\"><tr><td>{0}</td></tr></table>", longText));
                case TrialStringType.LongText:
                    return stringToModify.Insert(randomPosition, longText);
            }
        }

        /// <summary>
        /// Gets the random string.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            string characters = "abcdefghijkl mnopqrstu vwxyz, .:+ιθηΰ*ωµ~&=%;?  ";

            Random random = new Random();
            string randomString = string.Empty;
            string pattern = (random.Next(2) == 0 ? "TRIAL" : "EVAL");

            for (int index = 0; index < length - pattern.Length - 2; index++)
            {
                int randomIndex = random.Next(characters.Length);
                randomString += characters[randomIndex];
            }

            // Insert pattern
            randomString = randomString.Insert(random.Next(randomString.Length), pattern);

            return " " + randomString + " ";
        }
    }

    internal enum TrialStringType
    {
        LongText,
        LongHtml,
        ShortText
    }
}
