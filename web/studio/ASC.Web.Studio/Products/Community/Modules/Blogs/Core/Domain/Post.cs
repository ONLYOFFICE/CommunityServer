/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


#region Usings

using System;
using System.Collections.Generic;
using ASC.Blogs.Core.Security;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core.Users;
using HtmlAgilityPack;

#endregion

namespace ASC.Blogs.Core.Domain
{
    public enum BlogType
    {
        Personal,
        Corporate
    }

    public class Post : ISecurityObject
    {
        #region members

        //private UserInfo _User;
        private Guid id;
        private string content;
        private List<Tag> tagList = new List<Tag>();


        #endregion

        #region Properties

        public virtual BlogType BlogType
        {
            get { return BlogType.Personal; }
        }

        public virtual Guid ID
        {
            get { return id; }
            set { id = value; }
        }

        public virtual int AutoIncrementID { get; set; }


        public long BlogId { get; set; }

        public string BlogTitle { get; set; }

        public virtual Guid UserID { get; set; }

        public virtual string Title { get; set; }

        public virtual string Content
        {
            get { return content; }
            set { content = value; }
        }

        public virtual DateTime Datetime { get; set; }


        public virtual List<Tag> TagList
        {
            get { return tagList; }
            set { tagList = value; }
        }

        public virtual void ClearTags()
        {
            tagList.Clear();
        }

        public virtual UserInfo Author
        {
            get { return ASC.Core.CoreContext.UserManager.GetUsers(UserID); }
        }


        #endregion

        #region Methods

        public override int GetHashCode()
        {
            return (GetType().FullName + "|" + id).GetHashCode();
        }

        public virtual string GetPreviewText(int maxCharCount)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(this.content))
                return result;

            string content = ResizeImgForLetter(this.content).Replace("\r\n", "");

            IList<string> tagExcludeList = new List<string>();
            tagExcludeList.Add("img");
            tagExcludeList.Add("!--");
            tagExcludeList.Add("meta");
            tagExcludeList.Add("embed");
            tagExcludeList.Add("col");
            tagExcludeList.Add("input");
            tagExcludeList.Add("object");
            tagExcludeList.Add("hr");
            tagExcludeList.Add("br");
            tagExcludeList.Add("li");

            var tagList = new Stack<string>();

            int charCount = 0;
            int index = 0;

            while (index < content.Length)
            {
                int posBeginOpenTag = content.IndexOf("<", index);
                int posEndOpenTag = content.IndexOf(">", index);

                int posBeginCloseTag = content.IndexOf("</", index);
                int posEndCloseTag = content.IndexOf("/>", index);

                if (posBeginOpenTag == -1)
                {
                    AddHTMLText(content, index, content.Length - index, ref result, ref charCount, maxCharCount);
                    break;
                }
                if (index < posBeginOpenTag)
                {
                    if (AddHTMLText(content, index, posBeginOpenTag - index, ref result, ref charCount, maxCharCount) == 1)
                        break;
                    index = posBeginOpenTag;
                }
                else
                {
                    index = AddHTMLTag(tagExcludeList, content, posBeginOpenTag, posEndOpenTag, posBeginCloseTag, posEndCloseTag, ref result, ref tagList);
                    if (index == -1) break;
                }
            }


            while (tagList.Count != 0)
            {
                string temp = tagList.Pop();
                if (!tagExcludeList.Contains(temp.ToLower()))
                    result += "</" + temp + ">";
            }


            return result;
        }

        private int AddHTMLText(string sourceStr, int startPos, int len, ref string outStr, ref int charCount, int maxCharCount)
        {

            string str = sourceStr.Substring(startPos, len);
            if (str.Replace("&nbsp;", " ").Length + charCount > maxCharCount)
            {
                int dif = maxCharCount - charCount;
                int sublen = str.Replace("&nbsp;", " ").IndexOf(" ", dif);
                if (len > str.Replace("&nbsp;", " ").Length)
                    len = str.Replace("&nbsp;", " ").Length;
                outStr += str.Replace("&nbsp;", " ").Substring(0, (sublen == -1 ? len : sublen)).Replace("  ", "&nbsp;&nbsp;");
                if (sourceStr.Length > startPos + len)
                {
                    outStr += " ...";
                } return 1;
            }
            outStr += str;
            charCount += str.Replace("&nbsp;", " ").Length;
            return 0;
        }

        private int AddHTMLTag(IList<string> tagExcludeList, string sourceStr, int posBeginOpenTag, int posEndOpenTag, int posBeginCloseTag, int posEndCloseTag, ref string outStr, ref Stack<string> tagList)
        {
            if (posEndOpenTag == posEndCloseTag + 1)
            {
                if (sourceStr.Substring(posBeginOpenTag, posEndOpenTag - posBeginOpenTag + 1) == "<hr class=\"display-none\" />")
                {
                    return -1;
                }
                outStr += sourceStr.Substring(posBeginOpenTag, posEndOpenTag - posBeginOpenTag + 1);
                return posEndOpenTag + 1;

            }
            if (posBeginOpenTag == posBeginCloseTag)
            {
                string closeTag = sourceStr.Substring(posBeginCloseTag + 2, posEndOpenTag - posBeginCloseTag - 2);

                if (tagList.Peek() != closeTag)
                {
                    while (tagExcludeList.Contains(tagList.Peek().ToLower()))
                        tagList.Pop();
                }
                if (tagList.Peek() == closeTag)
                    outStr += "</" + tagList.Pop() + ">";
                else
                    outStr += "</" + closeTag + ">";
                return posEndOpenTag + 1;
            }
            string tagName = sourceStr.Substring(posBeginOpenTag + 1, posEndOpenTag - posBeginOpenTag - 1).Split(' ')[0];
            tagList.Push(tagName);

            outStr += sourceStr.Substring(posBeginOpenTag, posEndOpenTag - posBeginOpenTag + 1);
            return posEndOpenTag + 1;
        }

        public string ResizeImgForLetter(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(text);

                var imgs = doc.DocumentNode.SelectNodes(".//img");
                if (imgs != null)
                {
                    foreach (var img in imgs)
                    {
                        img.SetAttributeValue("style", "max-width: 540px;");
                    }
                    text = doc.DocumentNode.OuterHtml;
                }
            }
            return text;
        }


        #endregion

        #region ISecurityObjectId Members

        public Type ObjectType
        {
            get { return GetType(); }
        }

        public object SecurityId
        {
            get { return ID; }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();
            if (Equals(account.ID, UserID))
            {
                roles.Add(Common.Security.Authorizing.Constants.Owner);
            }
            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            return Equals(UserID, objectId.SecurityId) ? new PersonalBlogSecObject(Author) : null;
        }

        public bool InheritSupported
        {
            get { return true; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        public DateTime Updated { get; set; }

        #endregion
    }
}
