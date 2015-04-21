/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Runtime.Serialization;
using System.Diagnostics;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    [DataContract(Name = "operation_result", Namespace = "")]
    [DebuggerDisplay("Id = {Id}, Op = {OperationType}, Progress = {Progress}, Result = {Result}, Error = {Error}")]
    public class FileOperationResult
    {
        public FileOperationResult()
        {
            Id = String.Empty;
            Processed = String.Empty;
            Progress = 0;
            Error = String.Empty;
        }

        public FileOperationResult(FileOperationResult fileOperationResult)
        {
            Id = fileOperationResult.Id;
            OperationType = fileOperationResult.OperationType;
            Progress = fileOperationResult.Progress;
            Source = fileOperationResult.Source;
            Result = fileOperationResult.Result;
            Error = fileOperationResult.Error;
            Processed = fileOperationResult.Processed;
        }

        [DataMember(Name = "id", IsRequired = false)]
        public string Id { get; set; }

        [DataMember(Name = "operation", IsRequired = false)]
        public FileOperationType OperationType { get; set; }

        [DataMember(Name = "progress", IsRequired = false)]
        public int Progress { get; set; }

        [DataMember(Name = "source", IsRequired = false)]
        public string Source { get; set; }

        [DataMember(Name = "result", IsRequired = false)]
        public object Result { get; set; }

        [DataMember(Name = "error", IsRequired = false)]
        public string Error { get; set; }

        [DataMember(Name = "processed", IsRequired = false)]
        public string Processed { get; set; }

        public object[] FileIds { get; set; }
        public object[] FolderIds { get; set; }
    }
}