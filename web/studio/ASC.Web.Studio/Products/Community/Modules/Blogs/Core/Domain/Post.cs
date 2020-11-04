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
            content = ASC.Web.Studio.Utility.HtmlUtility.HtmlUtility.GetFull(content);

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
            if (posBeginOpenTag == posBeginCloseTag && tagList.Count > 0)
            {
                string closeTag = sourceStr.Substring(posBeginCloseTag + 2, posEndOpenTag - posBeginCloseTag - 2);

                if (tagList.Peek() != closeTag)
                {
                    while (tagList.Count > 0 && tagExcludeList.Contains(tagList.Peek().ToLower()))
                    {
                        tagList.Pop();
                    }
                }
                if (tagList.Count > 0 && tagList.Peek() == closeTag)
                    outStr += "</" + tagList.Pop() + ">";
                else
                    outStr += "</" + closeTag + ">";
                return posEndOpenTag + 1;
            }
            if (posEndOpenTag != -1)
            {
                string tagName = sourceStr.Substring(posBeginOpenTag + 1, posEndOpenTag - posBeginOpenTag - 1).Split(' ')[0];
                tagList.Push(tagName);

                outStr += sourceStr.Substring(posBeginOpenTag, posEndOpenTag - posBeginOpenTag + 1);
            }
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
