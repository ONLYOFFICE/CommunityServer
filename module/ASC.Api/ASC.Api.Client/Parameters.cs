/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
