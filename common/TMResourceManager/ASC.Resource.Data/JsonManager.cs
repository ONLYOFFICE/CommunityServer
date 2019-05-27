/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ASC.Common.Logging;
using Ionic.Zip;
using Newtonsoft.Json;
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
                jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            }

            var fileID = ResourceData.AddFile(fileName, projectName, moduleName);
            const string resourceType = "text";
            foreach (var key in jsonObj.Keys)
            {
                var word = new ResWord
                {
                    Title = key,
                    ValueFrom = jsonObj[key],
                    ResFile = new ResFile {FileID = fileID}
                };
                if (culture != "Neutral")
                {
                    var neutralKey = new ResWord
                    {
                        Title = key,
                        ValueFrom = jsonObj[key],
                        ResFile = new ResFile {FileID = fileID}
                    };

                    ResourceData.GetValueByKey(neutralKey, "Neutral");
                    if (string.IsNullOrEmpty(neutralKey.ValueTo)) continue;
                }

                ResourceData.AddResource(culture, resourceType, DateTime.UtcNow, word, true, "Console");
            }
        }

        public static string ExportJson(string project, string module, List<string> languages, string exportPath,
            bool withDefaultValue = true)
        {
            using (var fastZip = new ZipFile())
            {
                var filter = new ResCurrent
                {
                    Project = new ResProject {Name = project},
                    Module = new ResModule {Name = module}
                };

                var zipDirectory = Directory.CreateDirectory(exportPath + module);
                foreach (var language in languages)
                {
                    filter.Language = new ResCulture {Title = language};

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
                                var obj = JsonConvert.SerializeObject(wordsDictionary, Formatting.Indented);
                                writer.Write(obj);
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

                var zipPath = exportPath + "\\" + module + ".zip";
                fastZip.AddDirectory(zipDirectory.FullName);
                fastZip.Save(zipPath);

                zipDirectory.Delete(true);
                return zipPath;
            }
        }

        private static string GetCultureFromFileName(string fileName)
        {
            var culture = "Neutral";
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (nameWithoutExtension != null && nameWithoutExtension.Split('.').Length > 1)
            {
                culture = nameWithoutExtension.Split('.')[1];
            }

            return culture;
        }
    }
}