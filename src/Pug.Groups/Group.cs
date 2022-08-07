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

			_domain = info.Definition.Domain;
		}
		
		public async Task<GroupInfo> GetDefinitionAsync()
		{
			await CheckAuthorizationAsync(_domain, SecurityOperations.GetDefinition, SecurityObjectTypes.Group, Identifier);

			return await _GetDefinitionAsync();
		}
		
		public async Task<GroupInfo> GetInfoAsync()
		{
			await CheckAuthorizationAsync(_domain, SecurityOperations.GetInfo, SecurityObjectTypes.Group, Identifier);
			
			return await _GetInfoAsync();
		}

		public GroupInfo GetInfo()
		{
			CheckAuthorization(_domain, SecurityOperations.GetInfo, SecurityObjectTypes.Group, Identifier);
			
			return _GetInfo();
		}

		public async Task<IEnumerable<Membership>> GetMembershipsAsync()
		{
			await CheckAuthorizationAsync(_domain, SecurityOperations.ListMemberships, SecurityObjectTypes.Group, Identifier);
			
			return await _GetMembershipsAsync();
		}

		public async Task<bool> HasMemberAsync(Subject subject, bool recursive = false)
		{
			Helpers.ValidateParameter(subject, nameof(subject));
			
			await CheckAuthorizationAsync(_domain, SecurityOperations.ListMemberships, SecurityObjectTypes.Group, Identifier);
			
			return await _HasMemberAsync(subject, recursive);
		}

		public async Task AddMembersAsync(IEnumerable<Subject> subjects)
		{
			if( subjects == null || !subjects.Any() )
				return;

			foreach(Subject subject in subjects)
			{
				if( subject == null )
					continue;
				
				Helpers.ValidateParameter(subject, nameof(subjects));
			}
			
			await CheckAuthorizationAsync(_domain, SecurityOperations.CreateMembership, SecurityObjectTypes.Group, Identifier);

			await _AddMembersAsync(subjects);
		}

		public async Task RemoveMemberAsync(Subject subject)
		{
			if(subject == null) throw new ArgumentNullException(nameof(subject));
			
			Helpers.ValidateParameter(subject, nameof(subject));
			
			await CheckAuthorizationAsync(_domain, SecurityOperations.DeleteMembership, SecurityObjectTypes.Group, Identifier);

			await _RemoveMemberAsync(subject);
		}
	}
}