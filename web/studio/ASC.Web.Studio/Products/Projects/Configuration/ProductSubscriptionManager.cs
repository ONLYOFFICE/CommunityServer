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
using System.Linq;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects.Configuration
{
    public class ProductSubscriptionManager : IProductSubscriptionManager
    {
        private readonly Guid _newCommentForMessage = new Guid("{04EAEAB5-948D-4491-8CB9-493CDD37B52A}");
        private readonly Guid _newCommentForTask = new Guid("{3E732AFC-B786-48a0-A88D-F7C43A45942E}");

        private readonly Guid _taskClosed = new Guid("{6C0ED420-DDF1-41ff-8D40-93D3BF213FA3}");
        private readonly Guid _responsibleForTask = new Guid("{85502DA1-E681-4fc0-98A0-08BA4E58D435}");
        private readonly Guid _responsibleForMilestone = new Guid("{BEC3DFF6-FA2B-447a-A3D3-1A35477C9288}");
        private readonly Guid _responsibleForProject = new Guid("{E838ECA9-DC2E-4a06-A786-28958C9EEE2C}");
        private readonly Guid _inviteToProject = new Guid("{6C950DA7-C69C-4ed2-935F-E2E55B0318DF}");
        private readonly Guid _removeFromProject = new Guid("{F25BA11D-68EF-44cc-803B-EE96080FA87B}");

        private readonly Dictionary<Int32, Guid> _bindingProjectID = new Dictionary<Int32, Guid>();
        private readonly Dictionary<Int32, List<int>> _bindingMessageID = new Dictionary<Int32, List<int>>();
        private readonly Dictionary<Int32, List<int>> _bindingTaskID = new Dictionary<Int32, List<int>>();

        private readonly Dictionary<Int32, SubscriptionGroup> _bindingProjectToGroup = new Dictionary<Int32, SubscriptionGroup>();

        private int GetProjectIDByGroupID(Guid moduleOrGroupID)
        {
            var result = 0;
            if (moduleOrGroupID != Guid.Empty)
            {
                foreach (var item in _bindingProjectID.Where(item => item.Value == moduleOrGroupID))
                {
                    result = item.Key;
                }
            }
            return result;
        }

        private List<SubscriptionObject> GetNewCommentForMessageObjects(Guid moduleOrGroupID, bool getEntity)
        {
            var result = new List<SubscriptionObject>();
            var objects = new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForMessage));
            var subscriptionType = GetSubscriptionTypes().Find(item => item.ID == _newCommentForMessage);
            var filterProjectID = GetProjectIDByGroupID(moduleOrGroupID);
            var messageEngine = Global.EngineFactory.GetMessageEngine();

            foreach (var item in objects)
            {
                try
                {
                    if (item == null) continue;

                    var messageID = int.Parse(item.Split(new[] {'_'})[1]);
                    var projectID = int.Parse(item.Split(new[] {'_'})[2]);
                    if (filterProjectID > 0 && projectID != filterProjectID) continue;

                    if (getEntity)
                    {
                        var message = messageEngine.GetByID(messageID);
                        if (message != null && message.Project != null)
                        {
                            result.Add(new SubscriptionObject
                                           {
                                               ID = item,
                                               Name = message.Title,
                                               URL = String.Concat(PathProvider.BaseAbsolutePath,
                                                                   String.Format("messages.aspx?prjID={0}&id={1}",
                                                                                 message.Project.ID, message.ID)),
                                               SubscriptionGroup = _bindingProjectToGroup[projectID],
                                               SubscriptionType = subscriptionType
                                           });
                        }
                    }
                    else
                    {
                        if (_bindingProjectID.ContainsKey(projectID) && _bindingMessageID[projectID].Contains(messageID))
                        {
                            result.Add(new SubscriptionObject
                            {
                                ID = item,
                                SubscriptionGroup = _bindingProjectToGroup[projectID],
                                SubscriptionType = subscriptionType
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return result;
        }

        private List<SubscriptionObject> GetNewCommentForTaskObjects(Guid moduleOrGroupID, bool getEntity)
        {   
            var result = new List<SubscriptionObject>();
            var objects = new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForTask));
            var subscriptionType = GetSubscriptionTypes().Find(item => item.ID == _newCommentForTask);
            var filterProjectID = GetProjectIDByGroupID(moduleOrGroupID);
            var taskEngine = Global.EngineFactory.GetTaskEngine();
            foreach (var item in objects)
            {
                try
                {
                    if (item == null) continue;

                    var taskID = int.Parse(item.Split(new[] {'_'})[1]);
                    var projectID = int.Parse(item.Split(new[] {'_'})[2]);
                    if (filterProjectID > 0 && projectID != filterProjectID) continue;

                    if (getEntity)
                    {
                        var task = taskEngine.GetByID(taskID);
                        if (task != null && task.Project != null)
                        {
                            result.Add(new SubscriptionObject
                                           {
                                               ID = item,
                                               Name = task.Title,
                                               URL = String.Concat(PathProvider.BaseAbsolutePath,
                                                                   String.Format("tasks.aspx?prjID={0}&id={1}", task.Project.ID, task.ID)),
                                               SubscriptionGroup = _bindingProjectToGroup[projectID],
                                               SubscriptionType = subscriptionType
                                           });
                        }
                    }
                    else
                    {
                        if (_bindingProjectID.ContainsKey(projectID) && _bindingTaskID[projectID].Contains(taskID))
                        {
                            result.Add(new SubscriptionObject
                            {
                                ID = item,
                                SubscriptionGroup = _bindingProjectToGroup[projectID],
                                SubscriptionType = subscriptionType
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return result;
        }

        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            var objects = new List<SubscriptionObject>();
            objects.AddRange(GetNewCommentForMessageObjects(subItem, false));
            objects.AddRange(GetNewCommentForTaskObjects(subItem, false));
            return objects;
        }

        private List<SubscriptionObject> GetNewCommentForMessageObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
        {
            return GetNewCommentForMessageObjects(moduleOrGroupID, true);
        }

        private bool IsEmptyNewCommentForMessageSubscriptionType(Guid productID, Guid moduleOrGroupID, Guid typeID)
        {
            var objects = new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForMessage));

            foreach (var item in objects)
            {
                try
                {
                    if (item == null) continue;

                    var messageID = int.Parse(item.Split(new[] {'_'})[1]);
                    var projectID = int.Parse(item.Split(new[] {'_'})[2]);

                    if (!_bindingProjectID.ContainsKey(projectID)) continue;

                    var localGroupID = _bindingProjectID[projectID];
                    if (localGroupID != moduleOrGroupID) continue;

                    if(!_bindingMessageID[projectID].Contains(messageID)) continue;

                    return false;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return true;
        }

        private List<SubscriptionObject> GetNewCommentForTaskObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
        {
            return GetNewCommentForTaskObjects(moduleOrGroupID, true);
        }

        private bool IsEmptyNewCommentForTaskSubscriptionType(Guid productID, Guid moduleOrGroupID, Guid typeID)
        {
            var objects = new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForTask));

            foreach (var item in objects)
            {
                try
                {
                    if (item == null) continue;

                    var taskID = int.Parse(item.Split(new[] {'_'})[1]);
                    var projectID = int.Parse(item.Split(new[] {'_'})[2]);

                    if (!_bindingProjectID.ContainsKey(projectID)) continue;

                    var localGroupID = _bindingProjectID[projectID];
                    if (localGroupID != moduleOrGroupID) continue;

                    if (!_bindingTaskID[projectID].Contains(taskID)) continue;

                    return false;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return true;
        }


        public List<SubscriptionType> GetSubscriptionTypes()
        {

            return new List<SubscriptionType>
                       {
                           new SubscriptionType
                               {
                                   ID = _newCommentForMessage,
                                   Name = ProjectsCommonResource.NewCommentForMessage,
                                   NotifyAction = NotifyConstants.Event_NewCommentForMessage,
                                   Single = false,
                                   GetSubscriptionObjects = GetNewCommentForMessageObjects,
                                   IsEmptySubscriptionType = IsEmptyNewCommentForMessageSubscriptionType
                               },
                           new SubscriptionType
                               {
                                   ID = _newCommentForTask,
                                   Name = ProjectsCommonResource.NewCommentForTask,
                                   NotifyAction = NotifyConstants.Event_NewCommentForTask,
                                   Single = false,
                                   GetSubscriptionObjects = GetNewCommentForTaskObjects,
                                   IsEmptySubscriptionType = IsEmptyNewCommentForTaskSubscriptionType
                               },
                           new SubscriptionType
                               {
                                   ID = _taskClosed,
                                   Name = "Close task",
                                   NotifyAction = NotifyConstants.Event_TaskClosed,
                                   Single = false
                               },
                           new SubscriptionType
                               {
                                   ID = _responsibleForTask,
                                   Name = "Responsible for task",
                                   NotifyAction = NotifyConstants.Event_ResponsibleForTask,
                                   Single = false
                               },
                           new SubscriptionType
                               {
                                   ID = _responsibleForProject,
                                   Name = "Responsible for project",
                                   NotifyAction = NotifyConstants.Event_ResponsibleForProject,
                                   Single = false
                               },
                           new SubscriptionType
                               {
                                   ID = _responsibleForMilestone,
                                   Name = "Deadline milestone",
                                   NotifyAction = NotifyConstants.Event_MilestoneDeadline,
                                   Single = false
                               },

                           new SubscriptionType
                               {
                                   ID = _inviteToProject,
                                   Name = "Invite to project",
                                   NotifyAction = NotifyConstants.Event_InviteToProject,
                                   Single = false
                               },
                           new SubscriptionType
                               {
                                   ID = _removeFromProject,
                                   Name = "Remove from project",
                                   NotifyAction = NotifyConstants.Event_RemoveFromProject,
                                   Single = false
                               }
                       };
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return NotifySource.Instance.GetSubscriptionProvider(); }
        }

        public GroupByType GroupByType
        {
            get { return GroupByType.Groups; }
        }

        public List<SubscriptionGroup> GetSubscriptionGroups()
        {
            var preparateData = new List<string>();

            preparateData.AddRange(new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForTask)));
            preparateData.AddRange(new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForMessage)));

            var projects = Global.EngineFactory.GetProjectEngine().GetAll().OrderBy(r=> r.Title).ToList();
            var messages = Global.EngineFactory.GetMessageEngine().GetAll().ToList();
            var tasks = Global.EngineFactory.GetTaskEngine().GetAll().ToList();

            foreach (var item in preparateData)
            {
                try
                {
                    if (item == null) continue;

                    var subItems = item.Split(new[] {'_'});
                    var entityType = subItems[0];
                    var entityID = int.Parse(subItems[1]);
                    var projectID = int.Parse(subItems[2]);

                    var project = projects.Find(p => p.ID == projectID);
                    if (project == null) continue;

                    if (!_bindingProjectToGroup.ContainsKey(projectID))
                    {
                        var generatedProjectID = Guid.NewGuid();
                        _bindingProjectID.Add(projectID, generatedProjectID);
                        _bindingProjectToGroup.Add(projectID, new SubscriptionGroup {ID = generatedProjectID, Name = project.HtmlTitle});
                    }

                    if (entityType == "Message")
                    {
                        var message = messages.Find(m => m.ID == entityID);
                        if (message == null) continue;

                        if (!_bindingMessageID.ContainsKey(projectID))
                        {
                            _bindingMessageID.Add(projectID, new List<int> { entityID });
                        }
                        else
                        {
                            _bindingMessageID[projectID].Add(entityID);
                        }
                    }

                    if (entityType == "Task")
                    {
                        var task = tasks.Find(t => t.ID == entityID);
                        if (task == null) continue;

                        if (!_bindingTaskID.ContainsKey(projectID))
                        {
                            _bindingTaskID.Add(projectID, new List<int> { entityID });
                        }
                        else
                        {
                            _bindingTaskID[projectID].Add(entityID);
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }


            return new List<SubscriptionGroup>(_bindingProjectToGroup.Values);
        }

        private IEnumerable<string> GetSubscriptions(INotifyAction action)
        {
            return SubscriptionProvider.GetSubscriptions(action, NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));
        }
    }
}