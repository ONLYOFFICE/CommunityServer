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
using System.Text;

namespace ASC.Web.Core.Calendars
{
    public abstract class BaseTodo : ITodo, ICloneable
    {
        internal TimeZoneInfo TimeZone { get; set; }

        public BaseTodo()
        {
            this.Context = new TodoContext();
        }

        #region ITodo Members


        public virtual string CalendarId { get; set; }

        public virtual string Description { get; set; }

        public virtual string Id { get; set; }

        public virtual string Uid { get; set; }

        public virtual string Name { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual DateTime UtcStartDate { get; set; }

        public virtual TodoContext Context { get; set; }

        public virtual DateTime Completed { get; set; }


        #endregion

        #region ICloneable Members

        public object Clone()
        {
            var t = (BaseTodo)this.MemberwiseClone();
            t.Context = (TodoContext)this.Context.Clone();
            return t;
        }

        #endregion


        #region IiCalFormatView Members

        public virtual string ToiCalFormat()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("BEGIN:TODO");
            sb.AppendLine(String.Format("UID:{0}", string.IsNullOrEmpty(this.Uid) ? this.Id : this.Uid));
            sb.AppendLine(String.Format("SUMMARY:{0}", this.Name));

            if (!string.IsNullOrEmpty(this.Description))
                sb.AppendLine(String.Format("DESCRIPTION:{0}", this.Description.Replace("\n","\\n")));

            
            if (this.UtcStartDate != DateTime.MinValue)
                sb.AppendLine(String.Format("DTSTART:{0}", this.UtcStartDate.ToString("yyyyMMdd'T'HHmmss'Z'")));

            if (this.Completed != DateTime.MinValue)
                sb.AppendLine(String.Format("COMPLETED:{0}", this.Completed.ToString("yyyyMMdd'T'HHmmss'Z'")));

            sb.Append("END:TODO");
            return sb.ToString();
        }

        #endregion     
        
    }
}
