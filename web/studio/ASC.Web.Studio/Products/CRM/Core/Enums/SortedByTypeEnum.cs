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


namespace ASC.CRM.Core
{

    public enum TaskSortedByType
    {
        Title,
        Category,
        DeadLine,
        Contact,
        ContactManager
    }

    public enum DealSortedByType
    {
        Title,
        Responsible,
        Stage,
        BidValue,
        DateAndTime
    }

    public enum RelationshipEventByType
    {
        Created,
        CreateBy,
        Category,
        Content
    }

    public enum ContactSortedByType
    {
        DisplayName,
        ContactType,
        Created,
        FirstName, 
        LastName,
        History
    }

    public enum SortedByType
    {
        DateAndTime,
        Title,
        CreateBy
    }

    public enum InvoiceSortedByType
    {
        Number,
        IssueDate,
        Contact,
        DueDate,
        Status
    }

    public enum InvoiceItemSortedByType
    {
        Name,
        Price,
        Quantity,
        SKU,
        Created
    }

}
