// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Data;
#if !PocketPC
using System.Web.UI;
#endif
using System.Collections;
using System.ComponentModel;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// A collection of field format options.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class ConditionalCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public ConditionalCollection()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Add an Condition object in the collection.
        /// </summary>
        /// <param name="condition">The Condition.</param>
        public void Add(Condition condition)
        {
            List.Add(condition);
        }

        /// <summary>
        /// Add an Condition object in the collection based the region id, field, and value.
        /// </summary>
        /// <param name="regionid">The name of the field.</param>
        /// <param name="field">The name of the field.</param>
        /// <param name="aValue">The value to match.</param>
        public void Add(string regionid, string field, string aValue)
        {
            List.Add(new Condition(regionid, field, aValue));
        }

        /// <summary>
        /// Add an Condition object based on it's region id, field, operator to evaluate to the value and whether it is case sensitive.
        /// </summary>
        /// <param name="regionid">The ID of the region.</param>
        /// <param name="field">The name of the field.</param>
        /// <param name="aValue">The value to match.</param>
        /// <param name="aOperator">The operator for the comparison.</param>
        /// <param name="casesensitive">Whether the value is matched with case sensitivity.</param>
        public void Add(string regionid, string field, string aValue, OperatorType aOperator, bool casesensitive)
        {
            List.Add(new Condition(regionid, field, aValue, aOperator, casesensitive));
        }
        
        /// <summary>
        /// Remove the Condition object from the collection at the specified index position.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            // Check to see if there is a EmbeddedObject at the supplied index.
            if (index < Count || index >= 0)
            {
                List.RemoveAt(index); 
            }
        }

        /// <summary>
        /// Returns the Condition object at the specified index position in the collection.
        /// </summary>
        public Condition this[int index]
        {
            get
            {
                return (Condition) List[index];
            }
        }

        /// <summary>
        /// Returns the Condition of the specified field.
        /// </summary>
        public Condition this[string field]
        {
            get
            {
                foreach (Condition condition in List)
                {
                    if (condition.Field.ToLower() == field.ToLower())
                        return condition;
                }
                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified field is in the list.
        /// </summary>
        /// <param name="field">The name of the field.</param>
        /// <returns>true if the collection contain the specified field; false otherwise.</returns>
        public bool Contains(string field)
        {
            foreach(Condition condition in List)
            {
                if (condition.Field.ToLower() == field.ToLower())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified region is in the list.
        /// </summary>
        /// <param name="regionID">The id of the region.</param>
        /// <returns>true if the collection contain the specified field; false otherwise.</returns>
        public bool ContainsRegion(string regionID)
        {
            foreach(Condition condition in List)
            {
                if (condition.RegionID.ToLower() == regionID.ToLower())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the specified region is a match.
        /// </summary>
        /// <param name="regionID">The id of the region.</param>
        /// <returns>true if the collection contain the specified field; false otherwise.</returns>
        public bool RemoveRegion(string regionID)
        {
            foreach(Condition condition in List)
            {
                if ((condition.RegionID.ToLower() == regionID.ToLower()) && (!condition.Match))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Validate the condition against the values passed.
        /// </summary>
        public void ClearMatch()
        {
            foreach (Condition condition in List)
            {
                if (condition.Operator == OperatorType.NotExists) 
                {
                    condition.Match = true;
                }
                else 
                {
                    condition.Match = false;
                }
            }
        }
        /// <summary>
        /// Validate the condition against the values passed.
        /// </summary>
        /// <param name="field">The field to match.</param>
        /// <param name="aValue">The value to match.</param>
        public void Validate(string field, object aValue)
        {
            foreach (Condition condition in List)
            {
                if (condition.Field.ToLower() == field.ToLower())
                    condition.Validate(aValue);
            }
        }

        public void Validate(object dataSource)
        {
            try 
            {

                string dataBinderVal = string.Empty;
                System.Data.DataColumnCollection columns;
                IEnumerator items = GetEnumerator(dataSource);
                if (items != null)
                {
                    if ((dataSource is IListSource) && !(dataSource is DataRowView))
                    {
                        while(items.MoveNext())
                        {
                            Validate(Convert.ToString(DataBinder.Eval(items.Current, "Key")), Convert.ToString(DataBinder.Eval(items.Current, "Value")));
                        }
                    } 
                    else
                    {
                        columns = GetColumns(dataSource);
                        while(items.MoveNext())
                        {
                            foreach(System.Data.DataColumn column in columns)
                            {
                                Validate(column.ColumnName, Convert.ToString(DataBinder.GetPropertyValue(items.Current, column.ColumnName)));
                            }
                        }
                    }
                }
            }
            catch 
            {
                return;
            }
        }

        /// <summary>
        /// Get the columns for the specified data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The columns.</returns>
        private System.Data.DataColumnCollection GetColumns(object dataSource)
        {
            if (dataSource == null) return null;

            // IEnumerable & IListSource specific methods

            if (dataSource is System.Data.DataSet)
            {
                return ((System.Data.DataSet)dataSource).Tables[0].Columns;
            }

            if (dataSource is System.Data.DataTable)
            {
                return ((System.Data.DataTable)dataSource).Columns;
            }
    
            if (dataSource is System.Data.DataView)
            {
                return ((System.Data.DataView)dataSource).Table.Columns;
            }

            if (dataSource is System.Data.DataRowView)
            {
                return ((System.Data.DataRowView)dataSource).Row.Table.Columns;
            }

            if (dataSource is System.Data.DataRow)
            {
                return ((System.Data.DataRow)dataSource).Table.Columns;
            }

            return null;
        }


        /// <summary>
        /// Get the enumerator from the specified data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The enumerator.</returns>
        private IEnumerator GetEnumerator(object dataSource)
        {
            if (dataSource == null) return null;

            // Set the IEnumerator object
            IEnumerator items = null;
            
            // IEnumerable & IListSource specific methods
            if (dataSource is IEnumerable)
            {
                items = ((IEnumerable)dataSource).GetEnumerator();
            }

            if (dataSource is IListSource)
            {
                items = ((IListSource)dataSource).GetList().GetEnumerator();
            } 
    
            if (dataSource is System.Data.DataRow)
            {
                
                DataView dataView = new DataView(((System.Data.DataRow)dataSource).Table);
                ArrayList sourceList = new ArrayList();

                foreach (DataRowView drv in dataView)
                {
                    if (drv.Row == dataSource)
                    {
                        sourceList.Add(drv);
                        break;
                    }
                }
                //sourceList.Add(dataView[0]);
                items = sourceList.GetEnumerator();
            }

            // Return the IEnumerator
            return items;
        }
    }
}
