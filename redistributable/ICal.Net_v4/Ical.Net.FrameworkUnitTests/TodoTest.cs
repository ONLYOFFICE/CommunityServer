using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.FrameworkUnitTests
{
    [TestFixture]
    public class TodoTest
    {
        private const string _tzid = "US-Eastern";

        [Test, TestCaseSource(nameof(ActiveTodo_TestCases)), Category("Todo")]
        public void ActiveTodo_Tests(string calendarString, IList<KeyValuePair<CalDateTime, bool>> incoming)
        {
            var iCal = Calendar.Load(calendarString);
            ProgramTest.TestCal(iCal);
            var todo = iCal.Todos;

            foreach (var calDateTime in incoming)
            {
                var dt = calDateTime.Key;
                dt.TzId = _tzid;
                Assert.AreEqual(calDateTime.Value, todo[0].IsActive(dt));
            }
        }

        public static IEnumerable<ITestCaseData> ActiveTodo_TestCases()
        {
            var testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2200, 12, 31, 0, 0, 0), true)
            };
            yield return new TestCaseData(IcsFiles.Todo1, testVals)
                .SetName("Todo1");

            testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 8, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 8, 59, 59), false),

                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2200, 12, 31, 0, 0, 0), true),
            };
            yield return new TestCaseData(IcsFiles.Todo2, testVals)
                .SetName("Todo2");

            testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 8, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2200, 12, 31, 0, 0, 0), false),
            };
            yield return new TestCaseData(IcsFiles.Todo3, testVals).SetName("Todo3");

            testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 29, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 30, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 31, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 1, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 2, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 3, 9, 0, 0), false),

                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 4, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 5, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 6, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 7, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 8, 9, 0, 0), true),
            };
            yield return new TestCaseData(IcsFiles.Todo5, testVals).SetName("Todo5");

            testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 29, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 30, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 31, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 1, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 2, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 3, 9, 0, 0), false),

                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 4, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 5, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 6, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 7, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 8, 9, 0, 0), true),
            };
            yield return new TestCaseData(IcsFiles.Todo6, testVals).SetName("Todo6");

            testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 29, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 30, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 31, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 1, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 2, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 3, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 4, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 5, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 6, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 30, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 31, 9, 0, 0), false),

                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 1, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 2, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 3, 9, 0, 0), true),
            };
            yield return new TestCaseData(IcsFiles.Todo7, testVals).SetName("Todo7");

            testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 29, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 30, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 31, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 1, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 2, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 3, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 4, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 5, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 6, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 30, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 31, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 1, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 2, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 3, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 10, 10, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 11, 15, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 12, 5, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 1, 3, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 1, 4, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 1, 5, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 1, 6, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 1, 7, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 2, 1, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 2, 2, 8, 59, 59), false),

                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 2, 2, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 2, 3, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2007, 2, 4, 9, 0, 0), true),
            };
            yield return new TestCaseData(IcsFiles.Todo8, testVals).SetName("Todo8");

            testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 28, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 29, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 7, 30, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 17, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 18, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 19, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 7, 9, 0, 0), false),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 8, 8, 59, 59), false),

                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 8, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 9, 9, 9, 0, 0), true),
            };
            yield return new TestCaseData(IcsFiles.Todo9, testVals).SetName("Todo9");
        }

        [Test, TestCaseSource(nameof(CompletedTodo_TestCases)), Category("Todo")]
        public void CompletedTodo_Tests(string calendarString, IList<KeyValuePair<CalDateTime, bool>> incoming)
        {
            var iCal = Calendar.Load(calendarString);
            ProgramTest.TestCal(iCal);
            var todo = iCal.Todos;

            foreach (var calDateTime in incoming)
            {
                var dt = calDateTime.Key;
                dt.TzId = _tzid;
                Assert.AreEqual(calDateTime.Value, todo[0].IsCompleted(dt));
            }
        }

        public static IEnumerable<ITestCaseData> CompletedTodo_TestCases()
        {
            var testVals = new List<KeyValuePair<CalDateTime, bool>>
            {
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 07, 28, 8, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 07, 28, 9, 0, 0), true),
                new KeyValuePair<CalDateTime, bool>(new CalDateTime(2006, 8, 1, 0, 0, 0), true),
            };
            yield return new TestCaseData(IcsFiles.Todo4, testVals).SetName("Todo4");
        }

        [Test, Category("Todo")]
        public void Todo7_1()
        {
            var iCal = Calendar.Load(IcsFiles.Todo7);
            var todo = iCal.Todos;

            var items = new List<CalDateTime>
            {
                new CalDateTime(2006, 7, 28, 9, 0, 0, _tzid),
                new CalDateTime(2006, 8, 4, 9, 0, 0, _tzid),
                new CalDateTime(2006, 9, 1, 9, 0, 0, _tzid),
                new CalDateTime(2006, 10, 6, 9, 0, 0, _tzid),
                new CalDateTime(2006, 11, 3, 9, 0, 0, _tzid),
                new CalDateTime(2006, 12, 1, 9, 0, 0, _tzid),
                new CalDateTime(2007, 1, 5, 9, 0, 0, _tzid),
                new CalDateTime(2007, 2, 2, 9, 0, 0, _tzid),
                new CalDateTime(2007, 3, 2, 9, 0, 0, _tzid),
                new CalDateTime(2007, 4, 6, 9, 0, 0, _tzid)
            };

            var occurrences = todo[0].GetOccurrences(
                new CalDateTime(2006, 7, 1, 9, 0, 0),
                new CalDateTime(2007, 7, 1, 9, 0, 0)).OrderBy(o => o.Period.StartTime).ToList();

            // FIXME: Count is not properly restricting recurrences to 10.
            // What's going wrong here?
            Assert.AreEqual(
                items.Count,
                occurrences.Count,
                "TODO should have " + items.Count + " occurrences; it has " + occurrences.Count);

            for (var i = 0; i < items.Count; i++)
            {
                Assert.AreEqual(items[i], occurrences[i].Period.StartTime, "TODO should occur at " + items[i] + ", but does not.");
            }
        }
    }
}
