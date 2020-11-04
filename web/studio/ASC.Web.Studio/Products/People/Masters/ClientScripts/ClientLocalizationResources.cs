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


using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using ASC.Web.People.Core.Import;
using ASC.Web.People.Resources;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.People.Classes;
using ASC.Web.Studio.Core.Users;
using Resources;

namespace ASC.Web.People.Masters.ClientScripts
{
    public class ClientLocalizationResources : ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get { return "ASC.People.Resources"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            Func<ImportParameters, object> parametersTemplate = r => new {id = r.Id, title = r.Title()};

            return new List<KeyValuePair<string, object>>(3)
            {
                RegisterResourceSet("PeopleJSResource", PeopleJSResource.ResourceManager),
                RegisterResourceSet("PeopleResource", PeopleResource.ResourceManager),
                RegisterObject(new
                {
                    UserControlsCommonResource.NextPage,
                    UserControlsCommonResource.PreviousPage,
                    UserControlsCommonResource.NotBeUndone,
                    Resource.Warning,
                    Import = new {
                        Resource.ImportContactsFirstName,
                        Resource.ImportContactsEmptyFirstName,
                        Resource.ImportContactsInvalidFirstName,
                        Resource.ImportContactsLastName,
                        Resource.ImportContactsEmptyLastName,
                        Resource.ImportContactsInvalidLastName,
                        Resource.ImportContactsEmail,
                        Resource.ImportContactsEmptyEmail,
                        Resource.ImportContactsInvalidEmail,
                        Resource.ImportContactsFromFileErrorTooLarge,
                        ImportContactsFromFileError = String.Format(Resource.ImportContactsFromFileError.ReplaceSingleQuote(), "<br />"),
                        Resource.ImportContactsIncorrectFields,
                        ImportContactsEmptyData = String.Format(Resource.ImportContactsEmptyData.ReplaceSingleQuote().Replace("\n", ""), "<br />"),
                        Encoding = EncodingParameters.Encodng.Select(parametersTemplate).ToList(),
                        Separator = SeparatorParameters.Separator.Select(parametersTemplate).ToList(),
                        Delimiter = DelimiterParameters.Delimiter.Select(parametersTemplate).ToList(),
                        ImportFromWhat = GetImportFromList()
                    }
                })
            };
        }

        private static List<ImportDataSource> GetImportFromList()
        {
            var importFromList = new List<ImportDataSource>
                {
                    new ImportDataSource(ImportFromWhatEnum.FromFile, PeopleResource.ImportFromFile)
                };

            if (Studio.ThirdParty.ImportContacts.Google.Enable)
            {
                importFromList.Add(new ImportDataSource(ImportFromWhatEnum.Google, PeopleResource.ImportFromGoogle));
            }

            if (Studio.ThirdParty.ImportContacts.Yahoo.Enable)
            {
                importFromList.Add(new ImportDataSource(ImportFromWhatEnum.Yahoo, PeopleResource.ImportFromYahoo));
            }

            return importFromList;
        }

        public class ImportDataSource
        {
            public int id;
            public string title;

            public ImportDataSource(ImportFromWhatEnum itemType, string itemTitle)
            {
                id = (int)itemType;
                title = itemTitle;
            }
        }
    }


    public class ClientCustomResources : ClientScriptCustom
    {
        protected override string BaseNamespace
        {
            get { return "ASC.People.Resources"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                           new
                           {
                                AddDepartmentDlgTitle = CustomNamingPeople.Substitute<PeopleResource>("AddDepartmentDlgTitle").HtmlEncode(),
                                DepartmentMaster = CustomNamingPeople.Substitute<PeopleResource>("DepartmentMaster").HtmlEncode()
                           })
                   };
        }
    }
}