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

namespace ASC.Api.Publisher
{
    public delegate void DataAvailibleDelegate(object data, object userData);

    public class DataHandler : IDisposable
    {
        private readonly DataAvailibleDelegate _dataAvailible;
        public event DataAvailibleDelegate DataAvailible = null;
        private bool _isDisposed;
        public object UserData { get; set; }

        public DataHandler(object userData, DataAvailibleDelegate dataAvailible)
        {
            _dataAvailible = dataAvailible;
            UserData = userData;
            DataAvailible += dataAvailible;
        }

        ~DataHandler()
        {
            Dispose(false);
        }

        public void OnDataAvailible(object data)
        {
            var handler = DataAvailible;
            if (handler != null) handler(data, UserData);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                DataAvailible -= _dataAvailible;
                _isDisposed = true;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (DataHandler)) return false;
            return Equals((DataHandler) obj);
        }

        public bool Equals(DataHandler other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._dataAvailible, _dataAvailible) && Equals(other.UserData, UserData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_dataAvailible != null ? _dataAvailible.GetHashCode() : 0)*397) ^ (UserData != null ? UserData.GetHashCode() : 0);
            }
        }
    }
}