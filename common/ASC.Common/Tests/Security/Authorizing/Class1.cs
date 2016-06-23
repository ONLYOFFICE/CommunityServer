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


#if DEBUG
using System;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using System.Collections.Generic;

namespace ASC.Common.Tests.Security.Authorizing {

	public class Class1 {

		public int Id {
			get;
			set;
		}

		public Class1() { }

		public Class1(int id) {
			Id = id;
		}

		public override string ToString() {
			return Id.ToString();
		}

		public override bool Equals(object obj) {
			var class1 = obj as Class1;
			return class1 != null && Equals(class1.Id, Id);
		}

		public override int GetHashCode() {
			return Id.GetHashCode();
		}
	}

	public class Class1SecurityProvider : ISecurityObjectProvider {

		private readonly Type type1 = typeof(Class1);

		#region ISecurityObjectProvider Members

		public bool InheritSupported {
			get { return true; }
		}

		public ISecurityObjectId InheritFrom(ISecurityObjectId objectId) {
			if (objectId.ObjectType == type1) {
				if (objectId.SecurityId.Equals(2)) return new SecurityObjectId(1, type1);
			}

			return null;
		}

		public bool ObjectRolesSupported {
			get { return true; }
		}

		public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext) {
			var roles = new List<IRole>();

			if (objectId.ObjectType == type1) {
				if (objectId.SecurityId.Equals(1) && account.Equals(Domain.accountNik)) {
					roles.Add(Constants.Owner);
					roles.Add(Constants.Self);
				}
				if (objectId.SecurityId.Equals(3) && account.Equals(Domain.accountAnton)) {
					roles.Add(Constants.Owner);
				}
			}

			return roles;
		}

		#endregion
	}
}
#endif