/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;

namespace ASC.Feed.Aggregator
{
    public class Feed
    {
        public Feed()
        {
        }

        
        
        public bool Variate { get; private set; }

        public Feed(Guid author, DateTime date, bool variate = false)
        {
            Author = Helper.GetUser(author);
            Date = date;
            LastModifiedBy = Author.UserInfo.ID;

            Action = FeedAction.Created;
            Variate = variate;
        }

        public string Item { get; set; }

        public string ItemId { get; set; }

        public string Id
        {
            get { return string.Format("{0}_{1}", Item, ItemId); }
        }

        public UserWrapper Author { get; private set; }

        public Guid LastModifiedBy { get; set; }

        public DateTime Date { get; private set; }

        public string Product { get; set; }

        public string Module { get; set; }
        
        public string ExtraLocation { get; set; }
        
        public string ExtraLocationUrl { get; set; }

        public FeedAction Action { get; set; }

        public string Title { get; set; }

        public string ItemUrl { get; set; }

        public string Description { get; set; }

        public string AdditionalInfo { get; set; }

        public string AdditionalInfo2 { get; set; }

        public string AdditionalInfo3 { get; set; }

        public string AdditionalInfo4 { get; set; }

        public string Keywords { get; set; }

        public bool HasPreview { get; set; }

        public bool CanComment { get; set; }

        public object Target { get; set; }

        public string CommentApiUrl { get; set; }

        public IEnumerable<FeedComment> Comments { get; set; }

        public string GroupId { get; set; }
    }
}