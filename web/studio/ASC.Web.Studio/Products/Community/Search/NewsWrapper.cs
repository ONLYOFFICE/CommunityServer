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
using System.Linq;
using ASC.Core;
using ASC.ElasticSearch;
using ASC.Web.Community.News.Code;

namespace ASC.Web.Community.Search
{
    public sealed class NewsWrapper : Wrapper
    {
        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("LastModified")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("caption", 1)]
        public string Caption { get; set; }

        [Column("text", 2, charFilter: CharFilter.html | CharFilter.io)]
        public string Text { get; set; }

        protected override string Table { get { return "events_feed"; } }

        [Join(JoinTypeEnum.Sub, "tenant:tenant", "id:poll")]
        public List<PollVariantWrapper> PollVariantWrapper { get; set; }

        public static implicit operator NewsWrapper(Feed feed)
        {
            var result =  new NewsWrapper
            {
                Id = (int) feed.Id,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                Caption = feed.Caption,
                Text = feed.Text
            };

            var poll = feed as FeedPoll;
            if (poll != null)
            {
                result.PollVariantWrapper = poll.Variants.Select(r => new PollVariantWrapper {Name = r.Name}).ToList();
            }

            return result;
        }

        /*
         * }
            }
         * */
    }

    public sealed class PollVariantWrapper : Wrapper
    {
        [Column("name", 1)]
        public string Name { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "events_pollvariant"; } }
    }
}