/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            return SubscriptionProvider.GetSubscriptions(action, 
                NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false);
        }
    }
}