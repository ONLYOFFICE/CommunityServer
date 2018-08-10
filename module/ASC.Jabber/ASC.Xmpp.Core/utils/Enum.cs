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


namespace ASC.Xmpp.Core.utils
{
    /// <summary>
    ///   Provides helper functions for Enumerations.
    /// </summary>
    /// <remarks>
    ///   Extends the <see cref="T:System.Enum">System.Enum Class</see> .
    /// </remarks>
    /// <seealso cref="T:System.Enum">System.Enum Class</seealso>
    public class Enum
    {
#if CF
		#region << Enum.Parse() for CF, credits to OpenNetCF.net for this function>>
		/// <summary>
		/// Use this on CF 1.0, CF 2 includes Enum.Parse() now
		/// </summary>
		/// <param name="enumType"></param>
		/// <param name="value"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>        
        public static object Parse(System.Type enumType, string value, bool ignoreCase)
		{
			//throw an exception on null value
			if(value.TrimEnd(' ')=="")
			{
				throw new ArgumentException("value is either an empty string (\"\") or only contains white space.");
			}
			else
			{
				//type must be a derivative of enum
				if(enumType.BaseType==Type.GetType("System.Enum"))
				{
					//remove all spaces
					string[] memberNames = value.Replace(" ","").Split(',');
					
					//collect the results
					//we are cheating and using a long regardless of the underlying type of the enum
					//this is so we can use ordinary operators to add up each value
					//I suspect there is a more efficient way of doing this - I will update the code if there is
					long returnVal = 0;

					//for each of the members, add numerical value to returnVal
					foreach(string thisMember in memberNames)
					{
						//skip this string segment if blank
						if(thisMember!="")
						{
							try
							{
								if(ignoreCase)
								{
									returnVal += (long)Convert.ChangeType(enumType.GetField(thisMember, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase).GetValue(null),returnVal.GetType(), null);
								}
								else
								{
									returnVal += (long)Convert.ChangeType(enumType.GetField(thisMember, BindingFlags.Public | BindingFlags.Static).GetValue(null),returnVal.GetType(), null);
								}
							}
							catch
							{
								try
								{
									//try getting the numeric value supplied and converting it
									returnVal += (long)Convert.ChangeType(System.Enum.ToObject(enumType, Convert.ChangeType(thisMember, System.Enum.GetUnderlyingType(enumType), null)),typeof(long),null);
								}
								catch
								{
									throw new ArgumentException("value is a name, but not one of the named constants defined for the enumeration.");
								}
								//
							}
						}
					}


					//return the total converted back to the correct enum type
					return System.Enum.ToObject(enumType, returnVal);
				}
				else
				{
					//the type supplied does not derive from enum
					throw new ArgumentException("enumType parameter is not an System.Enum");
				}				
			}
		}
		#endregion
#endif

#if CF || CF_2 || SL
		public static string[] GetNames(System.Type enumType)
		{
			if(enumType.BaseType==Type.GetType("System.Enum"))
			{
				//get the public static fields (members of the enum)
				System.Reflection.FieldInfo[] fi = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
			
				//create a new enum array
				string[] names = new string[fi.Length];

				//populate with the values
				for(int iEnum = 0; iEnum < fi.Length; iEnum++)
				{
					names[iEnum] = fi[iEnum].Name;
				}

				//return the array
				return names;
			}
			else
			{
				//the type supplied does not derive from enum
				throw new ArgumentException("enumType parameter is not an System.Enum");
			}
		}
#endif
    }
}