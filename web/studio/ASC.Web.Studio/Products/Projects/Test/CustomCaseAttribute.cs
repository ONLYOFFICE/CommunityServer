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