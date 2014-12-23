using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Web.XmlTransform.Test
{
    [TestClass]
    public class XmlTransformTest
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void XmlTransform_Support_WriteToStream()
        {
            string src = CreateATestFile("Web.config", Properties.Resources.Web);
            string transformFile = CreateATestFile("Web.Release.config", Properties.Resources.Web_Release);
            string destFile = GetTestFilePath("MyWeb.config");

            //execute
            Microsoft.Web.XmlTransform.XmlTransformableDocument x = new Microsoft.Web.XmlTransform.XmlTransformableDocument();
            x.PreserveWhitespace = true;
            x.Load(src);

            Microsoft.Web.XmlTransform.XmlTransformation transform = new Microsoft.Web.XmlTransform.XmlTransformation(transformFile);

            bool succeed = transform.Apply(x);

            FileStream fsDestFile = new FileStream(destFile, FileMode.OpenOrCreate);
            x.Save(fsDestFile);

            //verify, we have a success transform
            Assert.AreEqual(true, succeed);

            //verify, the stream is not closed
            Assert.AreEqual(true, fsDestFile.CanWrite, "The file stream can not be written. was it closed?");

            //sanity verify the content is right, (xml was transformed)
            fsDestFile.Close();
            string content = File.ReadAllText(destFile);
            Assert.IsFalse(content.Contains("debug=\"true\""));
            
            List<string> lines = new List<string>(File.ReadLines(destFile));
            //sanity verify the line format is not lost (otherwsie we will have only one long line)
            Assert.IsTrue(lines.Count>10);

            //be nice 
            transform.Dispose();
            x.Dispose();
        }

        [TestMethod]
        public void XmlTransform_AttibuteFormatting()
        {
            Transform_TestRunner_ExpectSuccess(Properties.Resources.AttributeFormating_source,
                    Properties.Resources.AttributeFormating_transform,
                    Properties.Resources.AttributeFormating_destination,
                    Properties.Resources.AttributeFormatting_log);
        }

        [TestMethod]
        public void XmlTransform_TagFormatting()
        {
             Transform_TestRunner_ExpectSuccess(Properties.Resources.TagFormatting_source,
                    Properties.Resources.TagFormatting_transform,
                    Properties.Resources.TagFormatting_destination,
                    Properties.Resources.TagFormatting_log);
        }

        [TestMethod]
        public void XmlTransform_HandleEdgeCase()
        {
            //2 edge cases we didn't handle well and then fixed it per customer feedback.
            //    a. '>' in the attribute value
            //    b. element with only one character such as <p>
            Transform_TestRunner_ExpectSuccess(Properties.Resources.EdgeCase_source,
                    Properties.Resources.EdgeCase_transform,
                    Properties.Resources.EdgeCase_destination,
                    Properties.Resources.EdgeCase_log);
        }

        [TestMethod]
        public void XmlTransform_ErrorAndWarning()
        {
            Transform_TestRunner_ExpectFail(Properties.Resources.WarningsAndErrors_source,
                    Properties.Resources.WarningsAndErrors_transform,
                    Properties.Resources.WarningsAndErrors_log);
        }

        private void Transform_TestRunner_ExpectSuccess(string source, string transform, string baseline, string expectedLog)
        {
            string src = CreateATestFile("source.config", source);
            string transformFile = CreateATestFile("transform.config", transform);
            string baselineFile = CreateATestFile("baseline.config", baseline);
            string destFile = GetTestFilePath("result.config");
            TestTransformationLogger logger = new TestTransformationLogger();

            XmlTransformableDocument x = new XmlTransformableDocument();
            x.PreserveWhitespace = true;
            x.Load(src);

            Microsoft.Web.XmlTransform.XmlTransformation xmlTransform = new Microsoft.Web.XmlTransform.XmlTransformation(transformFile, logger);

            //execute
            bool succeed = xmlTransform.Apply(x);
            x.Save(destFile);
            xmlTransform.Dispose();
            x.Dispose();
            //test
            Assert.AreEqual(true, succeed);
            CompareFiles(destFile, baselineFile);
            CompareMultiLines(expectedLog, logger.LogText);
        }

        private void Transform_TestRunner_ExpectFail(string source, string transform, string expectedLog)
        {
            string src = CreateATestFile("source.config", source);
            string transformFile = CreateATestFile("transform.config", transform);
            string destFile = GetTestFilePath("result.config");
            TestTransformationLogger logger = new TestTransformationLogger();

            XmlTransformableDocument x = new XmlTransformableDocument();
            x.PreserveWhitespace = true;
            x.Load(src);

            Microsoft.Web.XmlTransform.XmlTransformation xmlTransform = new Microsoft.Web.XmlTransform.XmlTransformation(transformFile, logger);

            //execute
            bool succeed = xmlTransform.Apply(x);
            x.Save(destFile);
            xmlTransform.Dispose();
            x.Dispose();
            //test
            Assert.AreEqual(false, succeed);
            CompareMultiLines(expectedLog, logger.LogText);
        }

        private void CompareFiles(string baseLinePath, string resultPath)
        {
            string bsl;
            using (StreamReader sr = new StreamReader(baseLinePath))
            {
                bsl = sr.ReadToEnd();
            }

            string result;
            using (StreamReader sr = new StreamReader(resultPath))
            {
                result = sr.ReadToEnd();
            }

            CompareMultiLines(bsl, result);
        }

        private void CompareMultiLines(string baseline, string result)
        {
            string[] baseLines = baseline.Split(new string[] { System.Environment.NewLine },  StringSplitOptions.None);
            string[] resultLines = result.Split(new string[] { System.Environment.NewLine },  StringSplitOptions.None);

            for (int i = 0; i < baseLines.Length; i++)
            {
                Assert.AreEqual(baseLines[i], resultLines[i], string.Format("line {0} at baseline file is not matched", i));
            }
        }

        private string CreateATestFile(string filename, string contents)
        {
            string file = GetTestFilePath(filename);
            File.WriteAllText(file, contents);
            return file;
        }

        private string GetTestFilePath(string filename)
        {
            string folder = Path.Combine(TestContext.TestDeploymentDir, TestContext.TestName);
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, filename);
            return file;
        }
    }
}
