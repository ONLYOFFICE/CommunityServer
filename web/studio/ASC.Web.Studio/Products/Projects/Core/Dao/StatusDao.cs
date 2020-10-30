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
using ASC.Collections;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects;

namespace ASC.Projects.Data.DAO
{
    internal class CachedStatusDao : StatusDao
    {
        private readonly HttpRequestDictionary<List<CustomTaskStatus>> statusCache = new HttpRequestDictionary<List<CustomTaskStatus>>("status");

        public CachedStatusDao(int tenant) : base(tenant)
        {
        }

        public override void Delete(int id)
        {
            base.Delete(id);
            ResetCache();
        }

        public override CustomTaskStatus Create(CustomTaskStatus status)
        {
            var result = base.Create(status);
            ResetCache();
            return result;
        }

        public override void Update(CustomTaskStatus status)
        {
            base.Update(status);
            ResetCache();
        }

        public override List<CustomTaskStatus> Get()
        {
            return statusCache.Get(Tenant.ToString(), BaseGet);
        }

        private List<CustomTaskStatus> BaseGet()
        {
            return base.Get();
        }

        private void ResetCache()
        {
            statusCache.Reset(Tenant.ToString());
        }
    }

    internal class StatusDao : BaseDao, IStatusDao
    {
        public StatusDao(int tenant) : base(tenant)
        {
        }

        public virtual CustomTaskStatus Create(CustomTaskStatus status)
        {
            var insert = Insert(StatusTable, false)
                .InColumnValue("id", 0)
                .InColumnValue("title", status.Title)
                .InColumnValue("description", status.Description)
                .InColumnValue("statusType", status.StatusType)
                .InColumnValue("image", status.Image)
                .InColumnValue("imageType", status.ImageType)
                .InColumnValue("color", status.Color)
                .InColumnValue("`order`", status.Order)
                .InColumnValue("isDefault", status.IsDefault)
                .InColumnValue("available", status.Available)
                .Identity(1, 0, true);

            status.Id = Db.ExecuteScalar<int>(insert);
            return status;
        }

        public virtual void Update(CustomTaskStatus status)
        {
            var insert = Update(StatusTable)
                    .Set("title", status.Title)
                    .Set("description", status.Description)
                    .Set("image", status.Image)
                    .Set("imageType", status.ImageType)
                    .Set("color", status.Color)
                    .Set("`order`", status.Order)
                    .Set("statusType", status.StatusType)
                    .Set("available", status.Available)
                    .Where("id", status.Id);

            Db.ExecuteNonQuery(insert);
        }

        public virtual List<CustomTaskStatus> Get()
        {
            var insert = Query(StatusTable).Select("id", "title", "description", "image", "imageType", "color", "`order`", "statusType", "isDefault", "available");

            return Db.ExecuteList(insert)
                .ConvertAll(r => new CustomTaskStatus
                {
                    Id = Convert.ToInt32(r[0]),
                    Title = (string)r[1],
                    Description = (string)r[2],
                    Image = (string)r[3],
                    ImageType = (string)r[4],
                    Color = (string)r[5],
                    Order = Convert.ToInt32(r[6]),
                    StatusType = (TaskStatus)Convert.ToInt32(r[7]),
                    IsDefault = Convert.ToBoolean(r[8]),
                    Available = Convert.ToBoolean(r[9])
                });
        }

        public virtual void Delete(int id)
        {
            using (var tr = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(Update(TasksTable).Where("status_id", id).Set("status_id", null));
                Db.ExecuteNonQuery(Delete(StatusTable).Where("id", id));
                tr.Commit();
            }
        }
    }
}