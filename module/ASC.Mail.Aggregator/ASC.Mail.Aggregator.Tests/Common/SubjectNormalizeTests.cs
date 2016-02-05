/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

using System.Collections.Generic;
using ASC.Mail.Aggregator.Common.Utils;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common
{
    [TestFixture]
    class SubjectNormalizeTests
    {
        [Test]
        public void NormalizeVariousPrefixStyle()
        {
            string[] subjects =
            {
                "RE: subject of email",
                "RE[4]: subject of email",
                "[RE: subject of email]",
                "Re: [Fwd: ] subject of email",
                "Re: [Fwd: [Fwd: FW: subject of email]]",
                "FW: FW: (fwd) FW:  subject of email"
            };

            const string expected = "subject of email";

            foreach (var t in subjects)
            {
                var res = MailUtil.NormalizeSubject(t);
                Assert.AreEqual(expected, res);
            }
        }

        [Test]
        public void NormalizeVariousLanguages()
        {
            string[] subjects =
            {
                "R: RES: FYI: RIF: I: FS: VB: RV: ENC: subject of email",
                "PD: YNT: ILT: SV: VS: VL: BLS: TR: TRS: subject of email",
                "AW: WG: ΑΠ: ΣΧΕΤ: ΠΡΘ: ANTW: DOORST: НА: subject of email"
            };

            const string expected = "subject of email";

            foreach (var t in subjects)
            {
                var res = MailUtil.NormalizeSubject(t);
                Assert.AreEqual(expected, res);
            }
        }

        [Test]
        public void DeleteAbbreviations()
        {
            string[] subjects =
            {
                "R: [SPAM] subject of email",
                "[Y/N] PD: [RSVP] subject of email"
            };

            const string expected = "subject of email";

            foreach (var t in subjects)
            {
                var res = MailUtil.NormalizeSubject(t);
                Assert.AreEqual(expected, res);
            }
        }

        [Test]
        public void NormalizeComplexSubjects()
        {
            var subjectsAndExpects = new Dictionary<string, string>()
            {
                {
                    "Re: [CommunityServer] ONLYOFFICE run from visual studio 2012 goes into Redirection Loop (#28)",
                    "ONLYOFFICE run from visual studio 2012 goes into Redirection Loop (#28)"
                }
            };

            foreach (var t in subjectsAndExpects)
            {
                var res = MailUtil.NormalizeSubject(t.Key);
                Assert.AreEqual(t.Value, res);
            }
        }
    }
}
