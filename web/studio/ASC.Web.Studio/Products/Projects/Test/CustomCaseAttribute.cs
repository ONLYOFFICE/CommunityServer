/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System.Collections;
using NUnit.Framework;

namespace ASC.Web.Projects.Test
{
    public class TestCaseOwnerAttribute : TestCaseAttribute
    {
        public TestCaseOwnerAttribute() : base(BaseTest.Owner) { }
    }

    public class TestCaseAdminAttribute : TestCaseAttribute
    {
        public TestCaseAdminAttribute() : base(BaseTest.Admin) { }
    }

    public class TestCaseGuestAttribute : TestCaseAttribute
    {
        public TestCaseGuestAttribute() : base(BaseTest.Guest) { }
    }

    public class TestCaseUserNotInTeamAttribute : TestCaseAttribute
    {
        public TestCaseUserNotInTeamAttribute() : base(BaseTest.UserNotInTeam) { }
    }

    public class TestCaseUserInTeamAttribute : TestCaseAttribute
    {
        public TestCaseUserInTeamAttribute() : base(BaseTest.UserInTeam) { }
    }

    public class TestCaseProjectManagerAttribute : TestCaseAttribute
    {
        public TestCaseProjectManagerAttribute() : base(BaseTest.ProjectManager) { }
    }

    public class CustomTestCaseData
    {
        public static IEnumerable TestCases(string action)
        {
            yield return new TestCaseData(BaseTest.Owner).SetName(action + " Owner");
            yield return new TestCaseData(BaseTest.Admin).SetName(action + " Admin");
            yield return new TestCaseData(BaseTest.ProjectManager).SetName(action + " ProjectManager");
            yield return new TestCaseData(BaseTest.UserInTeam).SetName(action + " UserInTeam");
        }

        public static IEnumerable TestGuest(string action)
        {
            yield return new TestCaseData(BaseTest.Guest).SetName(action + " Guest");
            yield return new TestCaseData(BaseTest.UserNotInTeam).SetName(action + " User Not In Team");
        }
    }
}