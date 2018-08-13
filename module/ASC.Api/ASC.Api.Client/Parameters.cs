/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ASC.Api.Client
{
    public class RequestParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }

    public class RequestFile
    {
        public string Name { get; set; }

        public string ContentType { get; set; }

        public Stream Data { get; set; }

        public bool CloseStream { get; set; }
    }

    public class RequestParameterCollection : ICollection<RequestParameter>
    {
        private readonly Dictionary<string, RequestParameter> _parameters = new Dictionary<string, RequestParameter>();

        public void Add(RequestParameter item)
        {
            if (string.IsNullOrEmpty(item.Name))
                throw new ArgumentException("name of parameter can't be empty", "item");

            if (item.Value == null)
                throw new ArgumentException("value of parameter can't be null", "item");

            if (item.Name.EndsWith("[]"))
                item.Name = item.Name.Remove(item.Name.Length - 2);

            if (_parameters.ContainsKey(item.Name))
            {
                var enumerable1 = item.Value as IEnumerable;
                if (enumerable1 == null || (enumerable1 is string))
                    enumerable1 = new List<object> {item.Value};

                var item2 = _parameters[item.Name];
                var enumerable2 = item2.Value as IEnumerable;
                if (enumerable2 == null || (item2.Value is string))
                    enumerable2 = new List<object> {item2.Value};

                item.Value = enumerable1.Cast<object>().Concat(enumerable2.Cast<object>()).ToList();
            }

            _parameters[item.Name] = item;
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public bool Contains(RequestParameter item)
        {
            return _parameters.ContainsKey(item.Name);
        }

        public void CopyTo(RequestParameter[] array, int arrayIndex)
        {
            _parameters.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _parameters.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(RequestParameter item)
        {
            return _parameters.Remove(item.Name);
        }

        public IEnumerator<RequestParameter> GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RequestFileCollection : ICollection<RequestFile>
    {
        private readonly List<RequestFile> _files = new List<RequestFile>(); 

        public void Add(RequestFile item)
        {
            if (string.IsNullOrEmpty(item.Name))
                throw new ArgumentException("name of file can't be empty", "item");

            if (item.Data == null)
                throw new ArgumentException("file can't be empty", "item");

            if (string.IsNullOrEmpty(item.ContentType))
                item.ContentType = MimeMapping.GetMimeMapping(item.Name);

            _files.Add(item);
        }

        public void Clear()
        {
            _files.Clear();
        }

        public bool Contains(RequestFile item)
        {
            return _files.Contains(item);
        }

        public void CopyTo(RequestFile[] array, int arrayIndex)
        {
            _files.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _files.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(RequestFile item)
        {
            return _files.Remove(item);
        }

        public IEnumerator<RequestFile> GetEnumerator()
        {
            return _files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
