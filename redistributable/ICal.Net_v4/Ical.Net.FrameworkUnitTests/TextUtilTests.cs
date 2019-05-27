using System.Collections.Generic;
using Ical.Net.Utility;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.FrameworkUnitTests
{
    public class TextUtilTests
    {
        [Test, TestCaseSource(nameof(FoldLines_TestCases))]
        public string FoldLines_Tests(string incoming) => TextUtil.FoldLines(incoming);

        public static IEnumerable<ITestCaseData> FoldLines_TestCases()
        {
            yield return new TestCaseData("Short")
                .Returns("Short" + SerializationConstants.LineBreak)
                .SetName("Short string remains unfolded");

            const string moderatelyLongReturns =
                "HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHell" + SerializationConstants.LineBreak
                + " oWorld" + SerializationConstants.LineBreak;

            yield return new TestCaseData("HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorld")
                .Returns(moderatelyLongReturns)
                .SetName("Long string is folded");

            yield return new TestCaseData("          HelloWorldHelloWorldHelloWorldHelloWorldHelloWorld              ")
                .Returns("HelloWorldHelloWorldHelloWorldHelloWorldHelloWorld" + SerializationConstants.LineBreak)
                .SetName("Long string with front and rear whitespace is trimmed and fits in the allotted width");

            const string reallyLong =
                "HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorld";

            const string reallyLongReturns =
                "HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHell" + SerializationConstants.LineBreak
                + " oWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWo" + SerializationConstants.LineBreak
                + " rldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorld" + SerializationConstants.LineBreak
                + " HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHel" + SerializationConstants.LineBreak
                + " loWorldHelloWorld" + SerializationConstants.LineBreak;

            yield return new TestCaseData(reallyLong)
                .Returns(reallyLongReturns)
                .SetName("Really long string is split onto multiple lines at a width of 75 chars, prefixed with a space");
        }
    }
}
