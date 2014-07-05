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
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Data;

namespace ASC.Projects.Engine
{
    public class EngineFactory
    {
        public static readonly Guid ProductId = new Guid("1e044602-43b5-4d79-82f3-fd6208a11960");

        private readonly IDaoFactory daoFactory;
        private readonly string dbId;
        private readonly int tenantID;

        public bool DisableNotifications { get; set; }

        public EngineFactory(string dbId, int tenantID)
        {
            this.dbId = dbId;
            this.tenantID = tenantID;

            daoFactory = new DaoFactory(dbId, tenantID);
        }

        public FileEngine GetFileEngine()
        {
            return new FileEngine(dbId, tenantID);
        }

        public ProjectEngine GetProjectEngine()
        {
            return new CachedProjectEngine(daoFactory, this);
        }

        public MilestoneEngine GetMilestoneEngine()
        {
            return new MilestoneEngine(daoFactory, this);
        }

        public CommentEngine GetCommentEngine()
        {
            return new CommentEngine(daoFactory);
        }

        public SearchEngine GetSearchEngine()
        {
            return new SearchEngine(daoFactory);
        }

        public TaskEngine GetTaskEngine()
        {
            return new TaskEngine(daoFactory, this);
        }

        public SubtaskEngine GetSubtaskEngine()
        {
            return new SubtaskEngine(daoFactory, this);
        }

        public MessageEngine GetMessageEngine()
        {
            return new MessageEngine(daoFactory, this);
        }

        public TimeTrackingEngine GetTimeTrackingEngine()
        {
            return new TimeTrackingEngine(daoFactory);
        }

        public ParticipantEngine GetParticipantEngine()
        {
            return new ParticipantEngine(daoFactory);
        }

        public TagEngine GetTagEngine()
        {
            return new TagEngine(daoFactory);
        }

        public ReportEngine GetReportEngine()
        {
            return new ReportEngine(daoFactory);
        }

        public TemplateEngine GetTemplateEngine()
        {
            return new TemplateEngine(daoFactory, this);
        }
    }
}
