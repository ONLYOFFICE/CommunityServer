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
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using NotifySourceBase = ASC.Core.Notify.NotifySource;

namespace ASC.Web.Files.Services.NotifyService
{
    public class NotifySource : NotifySourceBase
    {
        private static NotifySource instance = new NotifySource();

        public static NotifySource Instance
        {
            get { return instance; }
        }

        public NotifySource()
            : base(new Guid("6FE286A4-479E-4c25-A8D9-0156E332B0C0"))
        {
        }

        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                NotifyConstants.Event_ShareFolder,
                NotifyConstants.Event_ShareDocument);
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(FilesPatternResource.patterns);
        }
    }
}