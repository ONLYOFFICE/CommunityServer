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