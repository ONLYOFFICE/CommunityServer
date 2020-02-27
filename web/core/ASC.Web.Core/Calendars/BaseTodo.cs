/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
