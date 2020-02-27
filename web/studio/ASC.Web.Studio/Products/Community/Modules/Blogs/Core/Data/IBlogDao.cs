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


using ASC.Blogs.Core.Domain;
using System;
using System.Collections.Generic;

namespace ASC.Blogs.Core.Data
{
    public interface IPostDao
    {
        List<Post> Select(Guid? id, long? blogId, Guid? userId, string tag, bool withContent, bool asc, int? from, int? count, bool fillTags, bool withCommentsCount);

        List<Post> Select(Guid? id, long? blogId, Guid? userId, bool withContent, bool asc, int? from, int? count, bool fillTags, bool withCommentsCount);
        
        List<Post> Select(Guid? id, long? blogId, Guid? userId, bool withContent, bool fillTags, bool withCommentsCount);

        List<Post> GetPosts(List<Guid> ids,bool withContent,bool withTags);
        List<Post> GetPosts(List<int> ids, bool withContent, bool withTags);

        List<int> SearchPostsByWord(string word);

        void SavePost(Post post);
        void DeletePost(Guid postId);

        int GetCount(Guid? id, long? blogId, Guid? userId, string tag);

        List<Comment> GetComments(Guid postId);
        Comment GetCommentById(Guid commentId);        
        List<int> GetCommentsCount(List<Guid> postsIds);

        void SaveComment(Comment comment);

        List<TagStat> GetTagStat(int? top);
        List<string> GetTags(string like, int limit);

        void SavePostReview(Guid userId, Guid postId, DateTime datetime);
    }
}
