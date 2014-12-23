/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Runtime.Serialization;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Configuration;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Api.CRM.Wrappers
{

    #region History Category

    [DataContract(Name = "historyCategoryBase", Namespace = "")]
    public class HistoryCategoryBaseWrapper : ListItemWrapper
    {
        public HistoryCategoryBaseWrapper() : base(0)
        {
        }

        public HistoryCategoryBaseWrapper(ListItem listItem)
            : base(listItem)
        {
            if (!String.IsNullOrEmpty(listItem.AdditionalParams))
                ImagePath = WebImageSupplier.GetAbsoluteWebPath(listItem.AdditionalParams, ProductEntryPoint.ID);
        }

        [DataMember]
        public String ImagePath { get; set; }

        public static HistoryCategoryBaseWrapper GetSample()
        {
            return new HistoryCategoryBaseWrapper
                {
                    ID = 30,
                    Title = "Lunch",
                    SortOrder = 10,
                    Color = String.Empty,
                    Description = "",
                    ImagePath = "path to image"
                };
        }
    }

    [DataContract(Name = "historyCategory", Namespace = "")]
    public class HistoryCategoryWrapper : HistoryCategoryBaseWrapper
    {
        public HistoryCategoryWrapper()
        {
        }

        public HistoryCategoryWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static HistoryCategoryWrapper GetSample()
        {
            return new HistoryCategoryWrapper
                {
                    ID = 30,
                    Title = "Lunch",
                    SortOrder = 10,
                    Color = String.Empty,
                    Description = "",
                    ImagePath = "path to image",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Deal Milestone

    [DataContract(Name = "opportunityStagesBase", Namespace = "")]
    public class DealMilestoneBaseWrapper : ListItemWrapper
    {
        public DealMilestoneBaseWrapper()
            : base(0)
        {
        }

        public DealMilestoneBaseWrapper(DealMilestone dealMilestone)
            : base(dealMilestone.ID)
        {
            SuccessProbability = dealMilestone.Probability;
            StageType = dealMilestone.Status;
            Color = dealMilestone.Color;
            Description = dealMilestone.Description;
            Title = dealMilestone.Title;
        }

        [DataMember]
        public int SuccessProbability { get; set; }

        [DataMember]
        public DealMilestoneStatus StageType { get; set; }

        public static DealMilestoneBaseWrapper GetSample()
        {
            return new DealMilestoneBaseWrapper
                {
                    ID = 30,
                    Title = "Discussion",
                    SortOrder = 2,
                    Color = "#B9AFD3",
                    Description = "The potential buyer showed his/her interest and sees how your offering meets his/her goal",
                    StageType = DealMilestoneStatus.Open,
                    SuccessProbability = 20
                };
        }
    }

    [DataContract(Name = "opportunityStages", Namespace = "")]
    public class DealMilestoneWrapper : DealMilestoneBaseWrapper
    {
        public DealMilestoneWrapper()
        {
        }

        public DealMilestoneWrapper(DealMilestone dealMilestone)
            : base(dealMilestone)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static DealMilestoneWrapper GetSample()
        {
            return new DealMilestoneWrapper
                {
                    ID = 30,
                    Title = "Discussion",
                    SortOrder = 2,
                    Color = "#B9AFD3",
                    Description = "The potential buyer showed his/her interest and sees how your offering meets his/her goal",
                    StageType = DealMilestoneStatus.Open,
                    SuccessProbability = 20,
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Task Category

    [DataContract(Name = "taskCategoryBase", Namespace = "")]
    public class TaskCategoryBaseWrapper : ListItemWrapper
    {
        public TaskCategoryBaseWrapper()
            : base(0)
        {
        }

        public TaskCategoryBaseWrapper(ListItem listItem)
            : base(listItem)
        {
            ImagePath = WebImageSupplier.GetAbsoluteWebPath(listItem.AdditionalParams, ProductEntryPoint.ID);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String ImagePath { get; set; }

        public static TaskCategoryBaseWrapper GetSample()
        {
            return new TaskCategoryBaseWrapper
                {
                    ID = 30,
                    Title = "Appointment",
                    SortOrder = 2,
                    Description = "",
                    ImagePath = "path to image"
                };
        }
    }

    [DataContract(Name = "taskCategory", Namespace = "")]
    public class TaskCategoryWrapper : TaskCategoryBaseWrapper
    {
        public TaskCategoryWrapper()
        {
        }

        public TaskCategoryWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static TaskCategoryWrapper GetSample()
        {
            return new TaskCategoryWrapper
                {
                    ID = 30,
                    Title = "Appointment",
                    SortOrder = 2,
                    Description = "",
                    ImagePath = "path to image",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Contact Status

    [DataContract(Name = "contactStatusBase", Namespace = "")]
    public class ContactStatusBaseWrapper : ListItemWrapper
    {
        public ContactStatusBaseWrapper() :
            base(0)
        {
        }

        public ContactStatusBaseWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        public static ContactStatusBaseWrapper GetSample()
        {
            return new ContactStatusBaseWrapper
                {
                    ID = 30,
                    Title = "Cold",
                    SortOrder = 2,
                    Description = ""
                };
        }
    }

    [DataContract(Name = "contactStatus", Namespace = "")]
    public class ContactStatusWrapper : ContactStatusBaseWrapper
    {
        public ContactStatusWrapper()
        {
        }

        public ContactStatusWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static ContactStatusWrapper GetSample()
        {
            return new ContactStatusWrapper
                {
                    ID = 30,
                    Title = "Cold",
                    SortOrder = 2,
                    Description = "",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Contact Type

    [DataContract(Name = "contactTypeBase", Namespace = "")]
    public class ContactTypeBaseWrapper : ListItemWrapper
    {
        public ContactTypeBaseWrapper() :
            base(0)
        {
        }

        public ContactTypeBaseWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        public static ContactTypeBaseWrapper GetSample()
        {
            return new ContactTypeBaseWrapper
                {
                    ID = 30,
                    Title = "Client",
                    SortOrder = 2,
                    Description = ""
                };
        }
    }

    [DataContract(Name = "contactType", Namespace = "")]
    public class ContactTypeWrapper : ContactTypeBaseWrapper
    {
        public ContactTypeWrapper()
        {
        }

        public ContactTypeWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static ContactTypeWrapper GetSample()
        {
            return new ContactTypeWrapper
                {
                    ID = 30,
                    Title = "Client",
                    SortOrder = 2,
                    Description = "",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Tags

    [DataContract(Name = "tagWrapper", Namespace = "")]
    public class TagWrapper
    {
        public TagWrapper()
        {
            Title = String.Empty;
            RelativeItemsCount = 0;
        }

        public TagWrapper(String tag, int relativeItemsCount = 0)
        {
            Title = tag;
            RelativeItemsCount = relativeItemsCount;
        }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public static TagWrapper GetSample()
        {
            return new TagWrapper
                {
                    Title = "Tag",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    [DataContract(Name = "listItem", Namespace = "")]
    public abstract class ListItemWrapper : ObjectWrapperBase
    {
        protected ListItemWrapper(int id)
            : base(id)
        {
        }

        protected ListItemWrapper(ListItem listItem)
            : base(listItem.ID)
        {
            Title = listItem.Title;
            Description = listItem.Description;
            Color = listItem.Color;
            SortOrder = listItem.SortOrder;
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Color { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SortOrder { get; set; }
    }
}