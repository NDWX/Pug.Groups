using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pug.Application;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Application.ServiceModel;
using Pug.Authorized;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public class Options
	{
		public string AdministratorUser { get; set; }
		
		public string AdministratorGroup { get; set; }
	}

	public class Groups : IGroups
	{
		private readonly IdentifierGenerator _identifierGenerator;
		private readonly ISessionUserIdentityAccessor _sessionUserIdentityAccessor;
		private readonly IApplicationData<IDataSession> _applicationDataProvider;
		private readonly IAuthorized _authorized;

		public Groups(Options options, IdentifierGenerator identifierGenerator,
					ISessionUserIdentityAccessor sessionUserIdentityAccessor,
					IApplicationData<IDataSession> applicationDataProvider,
					IApplicationData<Authorized.Data.IAuthorizedDataStore> authorizationDataStore)
		{
			_identifierGenerator = identifierGenerator ?? throw new ArgumentNullException(nameof(identifierGenerator));
			_sessionUserIdentityAccessor = sessionUserIdentityAccessor ?? throw new ArgumentNullException(nameof(sessionUserIdentityAccessor));
			_applicationDataProvider = applicationDataProvider ?? throw new ArgumentNullException(nameof(applicationDataProvider));

			_authorized = new Authorized.Authorized(
					new Authorized.Options()
					{
						AdministrativeUser = options.AdministratorUser,
						AdministratorRole = options.AdministratorGroup,
						AdministrativeActionGrantees = AdministrativeActionGrantees.Administrators |
														AdministrativeActionGrantees.AllowedUsers
					}, 
					new DefaultIdentifierGenerator(), 
					sessionUserIdentityAccessor, 
					new InternalUserRoleProvider(_applicationDataProvider), 
					authorizationDataStore
				);
		}
		
		public async Task<IEnumerable<GroupInfo>> GetGroupsAsync(string domain, Subject subject = null, bool recursive = false)
		{
			throw new System.NotImplementedException();
		}

		public async Task AddToGroupsAsync(Subject subject, string domain, IEnumerable<string> groups)
		{
			throw new System.NotImplementedException();
		}

		public async Task<bool> SubjectIsMemberAsync(Subject subject, string domain, string @group)
		{
			throw new System.NotImplementedException();
		}
	}
}