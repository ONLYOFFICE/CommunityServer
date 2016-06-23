/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Forum;
using ASC.Specific;

namespace ASC.Api.Forums
{
    [DataContract(Name = "thread", Namespace = "")]
    public class ForumThreadWrapperFull : ForumThreadWrapper
    {
       
        [DataMember(Order = 100)]
        public List<ForumTopicWrapper> Topics { get; set; }

        public ForumThreadWrapperFull(Thread thread, IEnumerable<Topic> topics):base(thread)
        {
            Topics = topics.Where(x=>x.IsApproved).Select(x => new ForumTopicWrapper(x)).ToList();
        }

        protected ForumThreadWrapperFull()
        {
        }

        public static new ForumThreadWrapperFull GetSample()
        {
            return new ForumThreadWrapperFull()
            {
                Created = ApiDateTime.GetSample(),
                Updated = ApiDateTime.GetSample(),
                Description = "Sample thread",
                Id = 10,
                UpdatedBy = EmployeeWraper.GetSample(),
                RecentTopicId = 1234,
                RecentTopicTitle = "Sample topic",
                Title = "The Thread",
                Topics = new List<ForumTopicWrapper>{ForumTopicWrapper.GetSample()}
            };
        }
    }
}