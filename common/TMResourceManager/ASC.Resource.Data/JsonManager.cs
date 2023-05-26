/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using ASC.Common.Logging;

using ICSharpCode.SharpZipLib.Zip;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TMResourceData.Model;

using Formatting = Newtonsoft.Json.Formatting;

namespace TMResourceData
{
    public static class JsonManager
    {
        public static void UploadJson(string fileName, Stream fileStream, string projectName, string moduleName)
        {
            var culture = GetCultureFromFileName(fileName);

            string jsonString;
            using (var reader = new StreamReader(fileStream))
            {
                jsonString = reader.ReadToEnd();
            }

            var jsonObj = new Dictionary<string, string>();

            if (Path.GetExtension(fileName) == ".xml")
            {
                var doc = new XmlDocument();
                doc.LoadXml(jsonString);
                var list = doc.SelectNodes("//resources//string");
                if (list != null)
                {
                    try
                    {
                        var nodes = list.Cast<XmlNode>().ToList();
                        jsonObj = nodes.ToDictionary(r => r.Attributes["name"].Value, r => r.InnerText);
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("ASC").ErrorFormat("parse xml " + fileName, e);
                    }
                }
            }
            else
            {
                var reader = JObject.Parse(jsonString).CreateReader();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.String)
                    {
                        var key = reader.Path;
                        key = Regex.Replace(Regex.Replace(key, @"\[\'(\s)*", ".$1"), @"(\s)*\'\]", "$1").TrimStart('.').TrimEnd('.');

                        if (reader.Value != null)
                        {
                            var value = reader.Value.ToString();
                            jsonObj.Add(key, value);
                        }
                    }
                }
            }

            var fileID = ResourceData.AddFile(fileName, projectName, moduleName);
            const string resourceType = "text";
            foreach (var key in jsonObj.Keys)
            {
                var word = new ResWord
                {
                    Title = key,
                    ValueFrom = jsonObj[key],
                    ResFile = new ResFile { FileID = fileID }
                };
                if (culture != "Neutral")
                {
                    var neutralKey = new ResWord
                    {
                        Title = key,
                        ValueFrom = jsonObj[key],
                        ResFile = new ResFile { FileID = fileID }
                    };

                    ResourceData.GetValueByKey(neutralKey, "Neutral");
                    if (string.IsNullOrEmpty(neutralKey.ValueTo)) continue;
                }

                ResourceData.AddResource(culture, resourceType, DateTime.UtcNow, word, true, "Console");
            }
        }

        public static string ExportJson(string project, string module, List<string> languages, string exportPath,
            bool withDefaultValue = true, bool withStructurJson = true)
        {
            var filter = new ResCurrent
            {
                Project = new ResProject { Name = project },
                Module = new ResModule { Name = module }
            };

            var zipDirectory = Directory.CreateDirectory(exportPath + module);

            foreach (var language in languages)
            {
                filter.Language = new ResCulture { Title = language };

                var words =
                    ResourceData.GetListResWords(filter, string.Empty).GroupBy(x => x.ResFile.FileID).ToList();
                if (!words.Any())
                {
                    Console.WriteLine("Error!!! Can't find appropriate project and module. Possibly wrong names!");
                    return null;
                }

                foreach (var fileWords in words)
                {
                    var wordsDictionary = new Dictionary<string, string>();
                    foreach (
                        var word in
                            fileWords.OrderBy(x => x.Title).Where(word => !wordsDictionary.ContainsKey(word.Title)))
                    {
                        if (string.IsNullOrEmpty(word.ValueTo) && !withDefaultValue) continue;

                        wordsDictionary[word.Title] = word.ValueTo ?? word.ValueFrom;
                        if (!string.IsNullOrEmpty(wordsDictionary[word.Title]))
                        {
                            wordsDictionary[word.Title] = wordsDictionary[word.Title].TrimEnd('\n').TrimEnd('\r');
                        }
                    }

                    if (!wordsDictionary.Any()) continue;

                    var firstWord = fileWords.FirstOrDefault();
                    var fileName = firstWord == null
                        ? module
                        : Path.GetFileNameWithoutExtension(firstWord.ResFile.FileName);
                    var ext = Path.GetExtension(firstWord.ResFile.FileName);

                    var zipFileName = zipDirectory.FullName + "\\" + fileName +
                                      (language == "Neutral" ? string.Empty : "." + language) + ext;
                    using (TextWriter writer = new StreamWriter(zipFileName))
                    {
                        if (ext == ".json")
                        {
                            if (withStructurJson)
                            {
                                var collectionNames = new List<string>();
                                var wrOrder = 0;
                                JObject jObject = null;
                                var writeJtoken = new JTokenWriter();

                                writeJtoken.WriteStartObject();
                                foreach (var vordsKV in wordsDictionary)
                                {
                                    var strNameSplit = vordsKV.Key.Split('.');

                                    for (var a = 0; a < strNameSplit.Length; a++)
                                    {
                                        while (collectionNames.Count < a + 1) collectionNames.Add("");

                                        if (collectionNames[a] != null && collectionNames[a] == strNameSplit[a])
                                            continue;
                                        if (wrOrder > a)
                                        {
                                            for (var b = a; b < collectionNames.Count; b++) collectionNames[b] = null;
                                            while (wrOrder > a)
                                            {
                                                writeJtoken.WriteEndObject();
                                                wrOrder--;
                                            }
                                        }
                                        writeJtoken.WritePropertyName(strNameSplit[a]);
                                        if (a < strNameSplit.Length - 1)
                                        {
                                            writeJtoken.WriteStartObject();
                                            wrOrder++;
                                            collectionNames[a] = strNameSplit[a];
                                        }
                                        else
                                            writeJtoken.WriteValue(vordsKV.Value);
                                    }
                                }
                                jObject = (JObject)writeJtoken.Token;
                                writer.Write(jObject);
                            }
                            else
                            {
                                var obj = JsonConvert.SerializeObject(wordsDictionary, Formatting.Indented);
                                writer.Write(obj);
                            }
                        }
                        else
                        {
                            var data = new XmlDocument();
                            var resources = data.CreateElement("resources");

                            foreach (var ind in wordsDictionary)
                            {
                                var stringAttr = data.CreateAttribute("name");
                                stringAttr.Value = ind.Key;

                                var child = data.CreateElement("string");
                                child.Attributes.Append(stringAttr);
                                child.InnerText = ind.Value;

                                resources.AppendChild(child);
                            }

                            data.AppendChild(resources);

                            var settings = new XmlWriterSettings
                            {
                                Indent = true,
                                IndentChars = "  ",
                                NewLineChars = Environment.NewLine,
                                NewLineHandling = NewLineHandling.Replace,
                                OmitXmlDeclaration = false,
                                ConformanceLevel = ConformanceLevel.Fragment
                            };

                            using (var xmlTextWriter = XmlWriter.Create(writer, settings))
                            {
                                data.WriteTo(xmlTextWriter);
                                xmlTextWriter.Flush();
                            }
                        }
                    }
                }
            }

            var zipPath = zipDirectory.FullName + ".zip";
            var fastZip = new FastZip();
            fastZip.CreateEmptyDirectories = true;
            fastZip.CreateZip(zipPath, zipDirectory.FullName, true, null);
            zipDirectory.Delete(true);

            return zipPath;
        }

        public static string GetJsonFromResx(string path)
        {
            var culture = GetCultureFromFileName(Path.GetFileName(path));

            var obj = XElement.Parse(File.ReadAllText(path))
                .Elements("data")
                .ToDictionary(el => el.Attribute("name").Value, el => el.Element("value").Value.Trim());

            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            return json;
        }

        public static Dictionary<string, string> GetResxDataFromJson(string path)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
        }

        public static string GetCultureFromFileName(string fileName)
        {
            var culture = "Neutral";
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (nameWithoutExtension != null && nameWithoutExtension.Split('.').Length > 1)
            {
                culture = nameWithoutExtension.Split('.')[1];
            }

            return culture;
        }

        public static string GetCultureAndFileName(string fileName)
        {
            var culture = "en";
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (nameWithoutExtension != null && nameWithoutExtension.Split('.').Length > 1)
            {
                culture = nameWithoutExtension.Split('.')[1];
                nameWithoutExtension = nameWithoutExtension.Split('.')[0];
            }

            return Path.Combine(culture, $"{nameWithoutExtension}.json");
        }
    }
}