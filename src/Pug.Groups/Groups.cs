using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
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

	internal class InternalUserRoleProvider : IUserRoleProvider
	{
		private readonly IApplicationData<IDataSession> _applicationData;

		public InternalUserRoleProvider(IApplicationData<IDataSession> applicationData)
		{
			_applicationData = applicationData;
		}
		
		public bool UserIsInRole(string user, string role)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						return Group.HasMember(
							context.role,
							new Subject()
								{ Identifier = context.user, Type = Authorized.SubjectTypes.User },
							true, dataSession);
					},
					new { user, role },
					TransactionScopeOption.Required,
					new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
				);
		}

		public bool UserIsInRoles(string user, ICollection<string> roles)
		{
			return _applicationData.Execute(
							(dataSession, context) =>
							{
								foreach(string role in context.roles)
								{
									if(!Group.HasMember(
											role,
											new Subject()
												{ Identifier = context.user, Type = Authorized.SubjectTypes.User },
											true, dataSession))
										return false;
								}

								return true;
							},
							new { user, roles },
							TransactionScopeOption.Required,
							new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
						);
		}

		internal static IEnumerable<string> GetMemberships(Subject subject, string domain, IDataSession dataSession, List<string> evaluatedGroups)
		{
			List<string> roles = new List<string>();
			
			IEnumerable<DirectMembership> memberships = dataSession.GetMemberships(subject, null);
			
			foreach(DirectMembership membership in memberships)
			{
				if(evaluatedGroups.Contains(membership.Group))
					continue;
				
				GroupInfo groupInfo = dataSession.GetGroupInfo(membership.Group);

				if(groupInfo.Domain == domain)
				{
					roles.Add(groupInfo.Identifier);
				}
				
				evaluatedGroups.Add(groupInfo.Identifier);
				
				roles.AddRange(
						GetMemberships(new Subject() {Type = SubjectTypes.GROUP, Identifier = groupInfo.Identifier}, domain, dataSession, evaluatedGroups)
				);
			}

			return roles;
		}
		
		public ICollection<string> GetUserRoles(string user, string domain)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						List<string> roles = new List<string>();
						
						IEnumerable<DirectMembership> memberships = dataSession.GetMemberships(new Subject() {Identifier = context.user, Type = SubjectTypes.USER}, null);

						foreach(DirectMembership membership in memberships)
						{
							GroupInfo groupInfo = dataSession.GetGroupInfo(membership.Group);

							if(groupInfo.Domain == context.domain)
							{
								roles.Add(groupInfo.Identifier);
							}
						}
					},
					new { user, domain },
					TransactionScopeOption.Required,
					new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
				);
		}
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