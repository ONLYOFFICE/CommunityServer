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
using System.Collections.Generic;

namespace ASC.Mail.Aggregator
{
    public class TasksConfig
    {
        private readonly TimeSpan _activeInterval;
        private readonly List<string> _workOnUsersOnly;
        private readonly bool _onlyTeamlabTasks;
        
        public TimeSpan ActiveInterval {
            get { return _activeInterval; }
        }

        public List<string> WorkOnUsersOnly
        {
            get { return _workOnUsersOnly; }
        }

        public bool OnlyTeamlabTasks
        {
            get { return _onlyTeamlabTasks; }
        }

        public class Builder
        {
            internal TimeSpan active_interval;
            internal List<string> work_on_users_only;
            internal bool only_teamlab_tasks;

            public virtual Builder SetActiveInterval(TimeSpan active_interval_obj)
            {
                active_interval = active_interval_obj;
                return this;
            }

            public virtual Builder SetUsersToWorkOn(List<string> work_on_users_only_obj)
            {
                work_on_users_only = work_on_users_only_obj;
                return this;
            }

            public virtual Builder SetOnlyTeamlabTasks(bool only_teamlab_tasks_obj)
            {
                only_teamlab_tasks = only_teamlab_tasks_obj;
                return this;
            }

            public TasksConfig Build()
            {
                return new TasksConfig(this);
            }
        }

        private TasksConfig(Builder builder)
        {
            _activeInterval = builder.active_interval;
            _workOnUsersOnly = builder.work_on_users_only;
            _onlyTeamlabTasks = builder.only_teamlab_tasks;
        }
    }
}
