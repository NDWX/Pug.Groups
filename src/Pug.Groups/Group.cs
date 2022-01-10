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
	public partial class Group :  GroupsBase, IGroup
	{
		public string Identifier { get; }

		public Group(string identifier, IApplicationData<IDataSession> applicationDataProvider, ISecurityManager securityManager)
		: base(applicationDataProvider, securityManager)
		{
			if(string.IsNullOrWhiteSpace(identifier))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(identifier));
			
			Identifier = identifier;

			GroupInfo info = _GetInfo();

			if(info == null)
				throw new UnknownGroupException(identifier);

			_domain = info.Domain;
		}
		
		public Task<GroupInfo> GetInfoAsync()
		{
			CheckAuthorization(_domain, SecurityOperations.GetInfo, SecurityObjectTypes.Group, Identifier);
			
			return _GetInfoAsync();
		}

		public GroupInfo GetInfo()
		{
			CheckAuthorization(_domain, SecurityOperations.GetInfo, SecurityObjectTypes.Group, Identifier);
			
			return _GetInfo();
		}

		public Task<IEnumerable<DirectMembership>> GetMembershipsAsync()
		{
			CheckAuthorization(_domain, SecurityOperations.ListMemberships, SecurityObjectTypes.Group, Identifier);
			
			return _GetMembershipsAsync();
		}

		public Task<bool> HasMemberAsync(Subject subject, bool recursive = false)
		{
			Helpers.ValidateParameter(subject, nameof(subject));
			
			CheckAuthorization(_domain, SecurityOperations.ListMemberships, SecurityObjectTypes.Group, Identifier);
			
			return _HasMemberAsync(subject, recursive);
		}

		public Task AddMembersAsync(IEnumerable<Subject> subjects)
		{
			if(subjects == null || !subjects.Any())
				return Task.CompletedTask;

			foreach(Subject subject in subjects)
			{
				if( subject == null )
					continue;
				
				Helpers.ValidateParameter(subject, nameof(subjects));
			}
			
			CheckAuthorization(_domain, SecurityOperations.CreateMembership, SecurityObjectTypes.Group, Identifier);

			return _AddMembersAsync(subjects);
		}

		public Task RemoveMemberAsync(Subject subject)
		{
			if(subject == null) throw new ArgumentNullException(nameof(subject));
			
			Helpers.ValidateParameter(subject, nameof(subject));
			
			CheckAuthorization(_domain, SecurityOperations.DeleteMembership, SecurityObjectTypes.Group, Identifier);

			return _RemoveMemberAsync(subject);
		}
	}
}