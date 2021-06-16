/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

namespace ASC.Forum
{
    public enum QuestionType
    {
        SeveralAnswer = 0,

        OneAnswer = 1
    }

    public class Question
    {
        public virtual int ID { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual QuestionType Type { get; set; }

        public virtual List<AnswerVariant> AnswerVariants { get; set; }

        public virtual int TopicID { get; set; }

        public virtual int TenantID { get; set; }

        public Question()
        {
            AnswerVariants = new List<AnswerVariant>(0);
        }
    }
}
