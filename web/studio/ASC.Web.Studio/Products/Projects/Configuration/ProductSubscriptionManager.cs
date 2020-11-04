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
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Core;
using ASC.Web.Projects.Resources;
using Autofac;

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
            using (var scope = DIHelper.Resolve())
            {
                var result = new List<SubscriptionObject>();
                var objects = new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForMessage));
                var subscriptionType = GetSubscriptionTypes().Find(item => item.ID == _newCommentForMessage);
                var filterProjectID = GetProjectIDByGroupID(moduleOrGroupID);
                var messageEngine = scope.Resolve<EngineFactory>().MessageEngine;

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
                                        String.Format("Messages.aspx?prjID={0}&id={1}",
                                            message.Project.ID, message.ID)),
                                    SubscriptionGroup = _bindingProjectToGroup[projectID],
                                    SubscriptionType = subscriptionType
                                });
                            }
                        }
                        else
                        {
                            if (_bindingProjectID.ContainsKey(projectID) &&
                                _bindingMessageID[projectID].Contains(messageID))
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
        }

        private List<SubscriptionObject> GetNewCommentForTaskObjects(Guid moduleOrGroupID, bool getEntity)
        {
            using (var scope = DIHelper.Resolve())
            {
                var result = new List<SubscriptionObject>();
                var objects = new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForTask));
                var subscriptionType = GetSubscriptionTypes().Find(item => item.ID == _newCommentForTask);
                var filterProjectID = GetProjectIDByGroupID(moduleOrGroupID);
                var taskEngine = scope.Resolve<EngineFactory>().TaskEngine;

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
                                        String.Format("Tasks.aspx?prjID={0}&id={1}", task.Project.ID, task.ID)),
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
            using (var scope = DIHelper.Resolve())
            {
                var factory = scope.Resolve<EngineFactory>();
                var preparateData = new List<string>();

                preparateData.AddRange(new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForTask)));
                preparateData.AddRange(new List<string>(GetSubscriptions(NotifyConstants.Event_NewCommentForMessage)));

                var projects = factory.ProjectEngine.GetAll().OrderBy(r => r.Title).ToList();
                var messages = factory.MessageEngine.GetAll().ToList();
                var tasks = factory.TaskEngine.GetAll().ToList();

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
                            _bindingProjectToGroup.Add(projectID,
                                new SubscriptionGroup {ID = generatedProjectID, Name = project.HtmlTitle});
                        }

                        if (entityType == "Message")
                        {
                            var message = messages.Find(m => m.ID == entityID);
                            if (message == null) continue;

                            if (!_bindingMessageID.ContainsKey(projectID))
                            {
                                _bindingMessageID.Add(projectID, new List<int> {entityID});
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
                                _bindingTaskID.Add(projectID, new List<int> {entityID});
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
        }

        private IEnumerable<string> GetSubscriptions(INotifyAction action)
        {
            return SubscriptionProvider.GetSubscriptions(action, 
                NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false);
        }
    }
}