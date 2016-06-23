using System.IO;
using System.Linq;
using ASC.Mail.Aggregator.Core.Clients;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common.MessageParserTests
{
    /// <summary>
    ///  This test creates right parsing xml. Use it for new test adding.
    /// </summary>
    [TestFixture]
    class ResultCreator : MessageParserTestsBase
    {
        [Test]
        [Ignore("This text need for right answers generation")]
        public void RecerateRight_ParsingResults()
        {
            var emlFiles = Directory.GetFiles(TestFolderPath, "*.eml")
                                     .Select(Path.GetFileName);

            foreach (var emlFile in emlFiles)
            {
                var emlMessage = MailClient.ParseMimeMessage(TestFolderPath + emlFile);
                CreateRightResult(emlMessage, RightParserResultsPath + emlFile.Replace(".eml", ".xml"));
            }
        }

        [Test]
        [Ignore("This text need for right answers generation")]
        public void RecerateRight_ParsingResult()
        {
            var emlFile = "empty_attach_body.eml";
            var emlMessage = MailClient.ParseMimeMessage(TestFolderPath + emlFile);
            CreateRightResult(emlMessage, RightParserResultsPath + emlFile.Replace(".eml", ".xml"));
        }
    }
}
