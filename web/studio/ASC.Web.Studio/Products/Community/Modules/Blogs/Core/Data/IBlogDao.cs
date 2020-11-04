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
