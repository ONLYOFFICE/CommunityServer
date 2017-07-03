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