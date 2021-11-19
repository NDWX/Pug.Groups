using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Authorized;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public class Group :  IGroup
	{
		private readonly string _identifier;
		private readonly IApplicationData<IDataSession> _applicationDataProvider;
		private readonly IAuthorized _authorized;

		public Group(string identifier, ISessionUserIdentityAccessor sessionUserIdentityAccessor, IApplicationData<IDataSession> applicationDataProvider, IAuthorized authorized)
		{
			if(string.IsNullOrWhiteSpace(identifier))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(identifier));
			
			_identifier = identifier;
			_applicationDataProvider = applicationDataProvider ?? throw new ArgumentNullException(nameof(applicationDataProvider));
			_authorized = authorized;
		}
		
		public async Task<GroupInfo> GetInfoAsync()
		{
			return await _applicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await dataSession.GetGroupInfoAsync(context.@this._identifier)
														.ConfigureAwait(false);
							},
							context: new {@this = this},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
							).ConfigureAwait(false);
		}
		
		public async Task<IEnumerable<DirectMembership>> GetMemberships()
		{
			return await _applicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await dataSession.GetMembershipsAsync(context.@this._identifier)
														.ConfigureAwait(false);
							},
							context: new {@this = this},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
						).ConfigureAwait(false);
		}

		internal static ICollection<string> GetMemberships(Subject subject, string domain, IDataSession dataSession, List<string> evaluatedGroups = null)
		{
			if(evaluatedGroups == null)
				evaluatedGroups = new List<string>();
			
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

		internal static async Task<bool> HasMemberAsync(string groupIdentifier, Subject subject, bool recursive, IDataSession dataSession, List<string> inspectedMemberGroups = null)
		{
			IEnumerable<DirectMembership> memberships = 
				await dataSession.GetMembershipsAsync(groupIdentifier)
								.ConfigureAwait(false);

			DirectMembership subjectMembership =
				memberships.FirstOrDefault(x => x.Subject == subject);

			if(subjectMembership != null)
				return true;

			if(!recursive)
				return false;

			if(inspectedMemberGroups == null)
				inspectedMemberGroups = new List<string>();

			IEnumerable<DirectMembership> groupMembers =
				memberships.Where(x => x.Subject.Type == SubjectTypes.GROUP);

			foreach(var groupMember in groupMembers)
			{
				if(inspectedMemberGroups.Contains(groupMember.Subject.Identifier))
					continue;
				
				// prevent current group from being inspected again in recursive inspection
				inspectedMemberGroups.Add(groupMember.Subject.Identifier);

				if(await HasMemberAsync(groupMember.Subject.Identifier, subject, recursive, dataSession,
										inspectedMemberGroups).ConfigureAwait(false))
					return true;
			}

			return false;
		}

		internal static bool HasMember(string groupIdentifier, Subject subject, bool recursive, IDataSession dataSession, List<string> inspectedMemberGroups = null)
		{
			IEnumerable<DirectMembership> memberships =
				dataSession.GetMemberships(groupIdentifier);

			DirectMembership subjectMembership =
				memberships.FirstOrDefault(x => x.Subject == subject);

			if(subjectMembership != null)
				return true;

			if(!recursive)
				return false;

			if(inspectedMemberGroups == null)
				inspectedMemberGroups = new List<string>();

			IEnumerable<DirectMembership> groupMembers =
				memberships.Where(x => x.Subject.Type == SubjectTypes.GROUP);

			foreach(var groupMember in groupMembers)
			{
				if(inspectedMemberGroups.Contains(groupMember.Subject.Identifier))
					continue;
				
				// prevent current group from being inspected again in recursive inspection
				inspectedMemberGroups.Add(groupMember.Subject.Identifier);

				if(HasMember(groupMember.Subject.Identifier, subject, recursive, dataSession,
										inspectedMemberGroups))
					return true;
			}

			return false;
		}

		public async Task<bool> HasMemberAsync(Subject subject, bool recursive = false)
		{
			return await _applicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await HasMemberAsync(context.@this._identifier, context.subject,
															context.recursive, dataSession).ConfigureAwait(false);
							},
							context: new {@this = this, subject, recursive},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
						).ConfigureAwait(false);
		}

		public async Task AddMembersAsync(IEnumerable<Subject> subject)
		{
			throw new System.NotImplementedException();
		}
	}
}