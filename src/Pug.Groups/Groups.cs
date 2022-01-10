using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups
{
	public partial class Groups : IGroups
	{
		public Groups(IdentifierGenerator identifierGenerator,
					IApplicationData<IDataSession> applicationDataProvider,
					ISecurityManager securityManager
		)
			: base(applicationDataProvider, securityManager)
		{
			_identifierGenerator = identifierGenerator ?? throw new ArgumentNullException(nameof(identifierGenerator));

		}

		public Task<string> AddGroupAsync(string domain, string name, string description)
		{
			if(domain == null) throw new ArgumentNullException(nameof(domain));
			if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

			CheckAuthorization( domain, SecurityOperations.Create, SecurityObjectTypes.Group);
			
			string identifier = _identifierGenerator.GetNext();
			
			GroupInfo groupInfo = new GroupInfo()
				{ Identifier = identifier, Domain = domain, Description = description ?? string.Empty, Name = name };
			
			return addGroupAsync(groupInfo);
		}

		public Task<IEnumerable<GroupInfo>> GetGroupsAsync(string domain, string name = null)
		{
			if(domain == null) throw new ArgumentNullException(nameof(domain));

			CheckAuthorization( domain, SecurityOperations.List, SecurityObjectTypes.Group);

			return getGroupsAsync(domain, name);
		}

		public Task<IGroup> GetGroupAsync(string identifier)
		{
			if(string.IsNullOrWhiteSpace(identifier))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(identifier));

			return getGroupAsync(identifier);
		}

		public Task DeleteGroup(string identifier)
		{
			if(string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(identifier));

			IGroup grp = new Group(identifier, _applicationDataProvider, _securityManager);
			GroupInfo info = grp.GetInfo();

			if(info == null)
				return Task.CompletedTask;
			
			CheckAuthorization( info.Domain, SecurityOperations.Delete, SecurityObjectTypes.Group, identifier);

			return deleteGroupAsync(identifier);
		}

		public Task<IEnumerable<DirectMembership>> GetMemberships(string domain, Subject subject,
																		bool recursive = false)
		{
			if(domain == null) throw new ArgumentNullException(nameof(domain));
			if(subject == null) throw new ArgumentNullException(nameof(subject));

			if(string.IsNullOrWhiteSpace(subject.Type) || string.IsNullOrWhiteSpace(subject.Identifier))
				throw new ArgumentException("Subject type and identifier must be specified", nameof(subject));
			
			CheckAuthorization( domain, SecurityOperations.ListMemberships, SecurityObjectTypes.Subject);

			return getMemberships(domain, subject, recursive);
		}

		public Task AddToGroupsAsync(Subject subject, IEnumerable<string> groups)
		{
			if(subject == null) throw new ArgumentNullException(nameof(subject));
			
			Helpers.ValidateParameter(subject, nameof(subject));

			if(groups == null || !groups.Any())
				return Task.CompletedTask;

			foreach(string group in groups)
			{
				IGroup grp = new Group(group, _applicationDataProvider, _securityManager);
				GroupInfo info = grp.GetInfo();

				if(info == null)
					throw new UnknownGroupException(group);

				CheckAuthorization( info.Domain, SecurityOperations.CreateMembership, SecurityObjectTypes.Group, group);
			}

			return addToGroupsAsync(subject, groups);
		}
	}
}