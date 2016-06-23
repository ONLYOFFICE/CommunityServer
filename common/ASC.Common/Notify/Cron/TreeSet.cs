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


#region usings

using System;
using System.Collections;

#endregion

namespace ASC.Notify.Cron
{

    #region

    #endregion

    [Serializable]
    public class TreeSet : ArrayList, ISortedSet
    {
        #region Members

        private readonly IComparer comparator = Comparer.Default;

        #endregion

        #region Properties

        public IComparer Comparator
        {
            get { return comparator; }
        }

        #endregion

        #region Constructor

        public TreeSet()
        {
        }

        public TreeSet(ICollection c)
        {
            AddAll(c);
        }

        public TreeSet(IComparer c)
        {
            comparator = c;
        }

        #endregion

        #region Methods

        public new bool Add(object obj)
        {
            bool inserted = AddWithoutSorting(obj);
            Sort(comparator);
            return inserted;
        }

        public bool AddAll(ICollection c)
        {
            IEnumerator e = new ArrayList(c).GetEnumerator();
            bool added = false;
            while (e.MoveNext())
            {
                if (AddWithoutSorting(e.Current))
                {
                    added = true;
                }
            }
            Sort(comparator);
            return added;
        }

        public object First()
        {
            return this[0];
        }

        public override bool Contains(object item)
        {
            IEnumerator tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
            {
                if (comparator.Compare(tempEnumerator.Current, item) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public ISortedSet TailSet(object limit)
        {
            ISortedSet newList = new TreeSet();
            int i = 0;
            while ((i < Count) && (comparator.Compare(this[i], limit) < 0))
            {
                i++;
            }
            for (; i < Count; i++)
            {
                newList.Add(this[i]);
            }
            return newList;
        }

        public static TreeSet UnmodifiableTreeSet(ICollection collection)
        {
            var items = new ArrayList(collection);
            items = ReadOnly(items);
            return new TreeSet(items);
        }

        #endregion

        #region Utility methods

        private bool AddWithoutSorting(object obj)
        {
            bool inserted;
            if (!(inserted = Contains(obj)))
            {
                base.Add(obj);
            }
            return !inserted;
        }

        #endregion
    }
}