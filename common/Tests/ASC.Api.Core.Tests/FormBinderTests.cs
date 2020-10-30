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


#if DEBUG
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ASC.Api.Core.Tests
{
    [TestClass]
    public class FormBinderTests
    {

        [TestInitialize]
        public void PreRun()
        {
            Bind1000GuidToCollection();//Warm up
        }

        [TestMethod]
        public void SimpleBind()
        {
            var nameValue = HttpUtility.ParseQueryString("files=11031&documents=111");
            var result = (int)Utils.Binder.Bind(typeof(int), nameValue, "files");
            var result2 = (int)Utils.Binder.Bind(typeof(int), nameValue, "documents");
            Assert.AreEqual(result, 11031);
            Assert.AreEqual(result2, 111);
        }

        [TestMethod]
        public void TestBinderWithSimpleArray()
        {
            var nameValue = HttpUtility.ParseQueryString("files[]=11031&files[]=111");
            var result = ((IEnumerable<int>)Utils.Binder.Bind(typeof(int[]), nameValue, "files")).ToArray();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length == 2);
            Assert.IsTrue(result[0] == 11031);
            Assert.IsTrue(result[1] == 111);
        }
        [TestMethod]
        public void TestBinderWithStringArray()
        {
            var nameValue = HttpUtility.ParseQueryString("strings[]=string1&strings[]=string2");
            var result = Utils.Binder.Bind<string[]>(nameValue, "strings");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length == 2);
            Assert.IsTrue(result[0] == "string1");
            Assert.IsTrue(result[1] == "string2");

        }

        [TestMethod]
        public void TestJsonBinding()
        {
            var complex = new TestComplex();
            complex.Name = "Aaabbb11";
            complex.SharingOptions = new[]
                                         {
                                             new SharingParam(){ActionId = "asfd",IsGroup = false,ItemId = Guid.NewGuid()},
                                             new SharingParam(){ActionId = "sdfsdg",IsGroup = true,ItemId = Guid.NewGuid()},
                                         };

            var collection = new NameValueCollection();
            var str = JsonConvert.SerializeObject(complex);
            var xdoc = JsonConvert.DeserializeXNode(str, "request", false);
            FillCollectionFromXElement(xdoc.Root.Elements(), string.Empty, collection);

            var binded = Utils.Binder.Bind<TestComplex>(collection);
        }

        [TestMethod]
        public void TestJsonSimpleBinding()
        {
            var collection = new NameValueCollection();
            var str = JsonConvert.SerializeObject(new { someData = new[] { 1,2,3,4} });
            var xdoc = JsonConvert.DeserializeXNode(str, "request", false);
            FillCollectionFromXElement(xdoc.Root.Elements(), string.Empty, collection);

            var binded = Utils.Binder.Bind<int[]>(collection, "someData");
        }

        private static void FillCollectionFromXElement(IEnumerable<XElement> elements, string prefix, NameValueCollection collection)
        {
            foreach (var grouping in elements.GroupBy(x => x.Name))
            {
                if (grouping.Count()<2)
                {
                    //Single element
                    var element = grouping.SingleOrDefault();
                    if (element != null)
                    {
                        if (!element.HasElements)
                        {
                            //Last one in tree
                            AddElement(prefix, collection, element);
                        }
                        else
                        {
                            FillCollectionFromXElement(element.Elements(), prefix + "." + element.Name.LocalName,collection);
                        }
                    }
                    
                }
                else
                {
                    //Grouping has more than one
                    if (grouping.All(x=>!x.HasElements))
                    {
                        //Simple collection
                        foreach (XElement element in grouping)
                        {
                            AddElement(prefix,collection,element);
                        }
                    }
                    else
                    {
                        var groupList = grouping.ToList();
                        for (int i = 0; i < groupList.Count; i++)
                        {
                            FillCollectionFromXElement(groupList[i].Elements(),prefix+"."+grouping.Key+"["+i+"]",collection);
                        }
                    }
                }
            }
        }

        private static void AddElement(string prefix, NameValueCollection collection, XElement element)
        {
            if (string.IsNullOrEmpty(prefix))
                collection.Add(element.Name.LocalName, element.Value);
            else
            {
                var prefixes = prefix.TrimStart('.').Split('.');
                string additional = string.Empty;
                if (prefixes.Length > 1)
                {
                    additional = string.Join("", prefix.Skip(1).Select(x => "[" + x + "]").ToArray());
                }
                collection.Add(prefixes[0]+additional + "[" + element.Name.LocalName + "]", element.Value);
            }
        }

        [TestMethod]
        public void TestBinderWithGuidArray()
        {
            var nameValue = HttpUtility.ParseQueryString("guids[]=2A0B1EB6-0B56-4641-A8D5-3AAE7E043E40&guids[]=DB01ED90-9E19-4c20-A454-20B9AEF4C579");
            var result = Utils.Binder.Bind<Guid[]>(nameValue, "guids");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length == 2);
            Assert.IsTrue(result[0] == new Guid("2A0B1EB6-0B56-4641-A8D5-3AAE7E043E40"));
            Assert.IsTrue(result[1] == new Guid("DB01ED90-9E19-4c20-A454-20B9AEF4C579"));

        }

        [TestMethod]
        public void BindSimpleToCollection()
        {
            var nameValue = HttpUtility.ParseQueryString("file=11031");
            var result = ((IEnumerable<int>)Utils.Binder.Bind(typeof(int[]), nameValue, "file")).ToArray();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestEmpty()
        {
            var nameValue = HttpUtility.ParseQueryString("subjects=");
            var result = ((IEnumerable<string>)Utils.Binder.Bind(typeof(IEnumerable<string>), nameValue, "subjects")).ToArray();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BindGuidToCollection()
        {
            var nameValue = HttpUtility.ParseQueryString("Title=dsadsa&Description=xzxzxzxczcxz&Responsible=93580c54-1132-4d6b-bf2d-da0bfaaa1a28&Responsibles%5B%5D=93580c54-1132-4d6b-bf2d-da0bfaaa1a28&Responsibles%5B%5D=0017794f-aeb7-49a5-8817-9e870e02bd3f&Responsibles%5B%5D=535e344c-a478-42c6-a0ed-4d46331193de&priority=0");
            var result = ((IEnumerable<Guid>)Utils.Binder.Bind(typeof(Guid[]), nameValue, "Responsibles")).ToArray();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Bind1000GuidToCollection()
        {
            for (int i = 0; i < 1000; i++)
            {
                var nameValue = HttpUtility.ParseQueryString("Title=dsadsa&Description=xzxzxzxczcxz&Responsible=93580c54-1132-4d6b-bf2d-da0bfaaa1a28&Responsibles%5B%5D=93580c54-1132-4d6b-bf2d-da0bfaaa1a28&Responsibles%5B%5D=0017794f-aeb7-49a5-8817-9e870e02bd3f&Responsibles%5B%5D=535e344c-a478-42c6-a0ed-4d46331193de&priority=0");
                var result = ((IEnumerable<Guid>)Utils.Binder.Bind(typeof(Guid[]), nameValue, "Responsibles")).ToArray();
                Assert.IsNotNull(result);
            }
        }

        public class SubscriptionState
        {
            public string Id { get; set; }
            public bool IsAccepted { get; set; }
        }

        [TestMethod]
        public void TestComplexClassBinding()
        {
            var query = @"states%5B0%5D%5BId%5D=Project_588&states%5B0%5D%5BIsAccepted%5D=false";
            var nameValue = HttpUtility.ParseQueryString(query);
            var result = (((List<SubscriptionState>)Utils.Binder.Bind(typeof(List<SubscriptionState>), nameValue, "states")));
        }

        [TestMethod]
        public void TestNullArrayComplexClassBinding()
        {
            var query = @"name=New+calendar&description=&textColor=rgb(0%2C+0%2C+0)&backgroundColor=rgb(135%2C+206%2C+250)&timeZone=Arabian+Standard+Time&alertType=0&hideEvents=true&sharingOptions%5B0%5D%5BActionId%5D=read&sharingOptions%5B0%5D%5BitemId%5D=646a6cff-df57-4b83-8ffe-91a24910328c&sharingOptions%5B0%5D%5BisGroup%5D=false";
            var nameValue = HttpUtility.ParseQueryString(query);
            var result = (((SharingParam[])Utils.Binder.Bind(typeof(SharingParam[]), nameValue, "sharingOptions")));
            Assert.AreNotEqual(string.Empty, result);
            Assert.AreEqual(result.Length, 1);
        }

        [TestMethod]
        public void TestComplexLowerCamelCaseClassBinding()
        {
            var query = @"Name=New+calendar&description=&textColor=rgb(0%2C+0%2C+0)&backgroundColor=rgb(135%2C+206%2C+250)&timeZone=Arabian+Standard+Time&alertType=0&hideEvents=true&sharingOptions%5B0%5D%5BActionId%5D=read&sharingOptions%5B0%5D%5BitemId%5D=646a6cff-df57-4b83-8ffe-91a24910328c&sharingOptions%5B0%5D%5BisGroup%5D=false";
            var nameValue = HttpUtility.ParseQueryString(query);
            var result = (((TestComplex)Utils.Binder.Bind(typeof(TestComplex), nameValue, "")));
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Name);
            Assert.AreNotEqual(string.Empty, result.SharingOptions);
            Assert.AreEqual(result.SharingOptions.Length, 1);

        }

        public class TestComplex
        {
            public string Name { get; set; }
            public SharingParam[] SharingOptions { get; set; }
        }

        public class SharingParam
        {
            public string ActionId { get; set; }

            public Guid ItemId { get; set; }

            public bool IsGroup { get; set; }
        }
    }
}
#endif