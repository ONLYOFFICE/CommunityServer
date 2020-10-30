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