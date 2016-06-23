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
using System.Collections.Generic;
using ASC.Common.Security;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;

namespace ASC.Common.Tests.Security.Authorizing
{
	class UserAccount : Account, IUserAccount
	{
		public UserAccount(Guid id, string name)
			: base(id, name, true)
		{
		}

		public string FirstName
		{
            get;
            private set;
        }

		public string LastName
		{
            get;
            private set;
        }

		public string Title
		{
            get;
            private set;
        }

		public string Department
		{
            get;
            private set;
        }

		public string Email
		{
            get;
            private set;
        }

		public int Tenant
		{
            get;
            private set;
        }
	}

	class AccountS : UserAccount
	{

		public AccountS(Guid id, string name)
			: base(id, name)
		{
		}
	}

	class Role : IRole
	{
		public Role(Guid id, string name)
		{
			ID = id;
			Name = name;
		}

		public Guid ID
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public bool IsAuthenticated
		{
			get;
			private set;
		}

		public string AuthenticationType
		{
			get;
			private set;
		}

		public override string ToString()
		{
			return string.Format("Role: {0}", Name);
		}
	}


	sealed class RoleFactory : IRoleProvider
	{

		public readonly Dictionary<ISubject, List<IRole>> AccountRoles = new Dictionary<ISubject, List<IRole>>(10);

		public readonly Dictionary<IRole, List<ISubject>> RoleAccounts = new Dictionary<IRole, List<ISubject>>(10);

		public void AddAccountInRole(ISubject account, IRole role)
		{
			List<IRole> roles = null;
			if (!AccountRoles.TryGetValue(account, out roles))
			{
				roles = new List<IRole>(10);
				AccountRoles.Add(account, roles);
			}
			if (!roles.Contains(role)) roles.Add(role);

			List<ISubject> accounts = null;
			if (!RoleAccounts.TryGetValue(role, out accounts))
			{
				accounts = new List<ISubject>(10);
				RoleAccounts.Add(role, accounts);
			}
			if (!accounts.Contains(account)) accounts.Add(account);
		}

		#region IRoleProvider Members

		public List<IRole> GetRoles(ISubject account)
		{
			List<IRole> roles = null;
			if (!AccountRoles.TryGetValue(account, out roles)) roles = new List<IRole>();
			return roles;
		}

		public List<ISubject> GetSubjects(IRole role)
		{
			List<ISubject> accounts = null;
			if (!RoleAccounts.TryGetValue(role, out accounts)) accounts = new List<ISubject>();
			return accounts;
		}

		public bool IsSubjectInRole(ISubject account, IRole role)
		{
			List<IRole> roles = GetRoles(account);
			return roles.Contains(role);
		}



		#endregion
	}

	sealed class PermissionFactory : IPermissionProvider
	{

		private readonly Dictionary<string, PermissionRecord> permRecords = new Dictionary<string, PermissionRecord>();

		private readonly Dictionary<string, bool> inheritAces = new Dictionary<string, bool>();


		public void AddAce(ISubject subject, IAction action, AceType reaction)
		{
			AddAce(subject, action, null, reaction);
		}

		public void AddAce(ISubject subject, IAction action, ISecurityObjectId objectId, AceType reaction)
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (action == null) throw new ArgumentNullException("action");

			var r = new PermissionRecord(subject.ID, action.ID, AzObjectIdHelper.GetFullObjectId(objectId), reaction);
			if (!permRecords.ContainsKey(r.Id))
			{
				permRecords.Add(r.Id, r);
			}
		}

		public void RemoveAce<T>(ISubject subject, IAction action, ISecurityObjectId objectId, AceType reaction)
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (action == null) throw new ArgumentNullException("action");

			var r = new PermissionRecord(subject.ID, action.ID, AzObjectIdHelper.GetFullObjectId(objectId), reaction);
			if (permRecords.ContainsKey(r.Id))
			{
				permRecords.Remove(r.Id);
			}
		}

		public void SetObjectAcesInheritance(ISecurityObjectId objectId, bool inherit)
		{
			var fullObjectId = AzObjectIdHelper.GetFullObjectId(objectId);
			inheritAces[fullObjectId] = inherit;
		}

		public bool GetObjectAcesInheritance(ISecurityObjectId objectId)
		{
			var fullObjectId = AzObjectIdHelper.GetFullObjectId(objectId);
			return inheritAces.ContainsKey(fullObjectId) ? inheritAces[fullObjectId] : true;
		}

		#region IPermissionProvider Members

		public IEnumerable<Ace> GetAcl(ISubject subject, IAction action)
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (action == null) throw new ArgumentNullException("action");

			var acl = new List<Ace>();
			foreach (var entry in permRecords)
			{
				var pr = entry.Value;
				if (pr.SubjectId == subject.ID && pr.ActionId == action.ID && pr.ObjectId == null)
				{
					acl.Add(new Ace(action.ID, pr.Reaction));
				}
			}
			return acl;
		}

        public IEnumerable<Ace> GetAcl(ISubject subject, IAction action, ISecurityObjectId objectId, ISecurityObjectProvider secObjProvider)
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (action == null) throw new ArgumentNullException("action");
			if (objectId == null) throw new ArgumentNullException("objectId");

			var allAces = new List<Ace>();
			var fullObjectId = AzObjectIdHelper.GetFullObjectId(objectId);

			allAces.AddRange(GetAcl(subject, action, fullObjectId));

			bool inherit = GetObjectAcesInheritance(objectId);
			if (inherit)
			{
				var providerHelper = new AzObjectSecurityProviderHelper(objectId, secObjProvider);
				while (providerHelper.NextInherit())
				{
					allAces.AddRange(GetAcl(subject, action, AzObjectIdHelper.GetFullObjectId(providerHelper.CurrentObjectId)));
				}
				allAces.AddRange(GetAcl(subject, action));
			}

			var aces = new List<Ace>();
			var aclKeys = new List<string>();
			foreach (var ace in allAces)
			{
				var key = string.Format("{0}{1:D}", ace.ActionId, ace.Reaction);
				if (!aclKeys.Contains(key))
				{
					aces.Add(ace);
					aclKeys.Add(key);
				}
			}

			return aces;
		}

		public ASC.Common.Security.Authorizing.Action GetActionBySysName(string sysname)
		{
			throw new NotImplementedException();
		}



		#endregion

		private List<Ace> GetAcl(ISubject subject, IAction action, string fullObjectId)
		{
			var acl = new List<Ace>();
			foreach (var entry in permRecords)
			{
				var pr = entry.Value;
				if (pr.SubjectId == subject.ID && pr.ActionId == action.ID && pr.ObjectId == fullObjectId)
				{
					acl.Add(new Ace(action.ID, pr.Reaction));
				}
			}
			return acl;
		}

		class PermissionRecord
		{
			public string Id;

			public Guid SubjectId;
			public Guid ActionId;
			public string ObjectId;
			public AceType Reaction;

			public PermissionRecord(Guid subjectId, Guid actionId, string objectId, AceType reaction)
			{
				SubjectId = subjectId;
				ActionId = actionId;
				ObjectId = objectId;
				Reaction = reaction;
				Id = string.Format("{0}{1}{2}{3:D}", SubjectId, ActionId, ObjectId, Reaction);
			}

			public override int GetHashCode()
			{
				return Id.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var p = obj as PermissionRecord;
				return p != null && Id == p.Id;
			}
		}
	}
}
#endif