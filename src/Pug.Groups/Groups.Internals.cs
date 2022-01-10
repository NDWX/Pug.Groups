using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups
{
	public partial class Groups : GroupsBase
	{
		private readonly IdentifierGenerator _identifierGenerator;

		private async Task<string> _AddGroupAsync(GroupInfo groupInfo)
		{
			return await ApplicationDataProvider.ExecuteAsync(
							async (session, context) =>
							{
								await session.InsertAsync(context.groupInfo).ConfigureAwait(false);

								return context.groupInfo.Identifier;
							},
							context: new { groupInfo },
							TransactionScopeOption.Required,
							new TransactionOptions()
							{
								IsolationLevel = IsolationLevel.ReadCommitted
							}
						);
		}

		private async Task<IEnumerable<GroupInfo>> _GetGroupsAsync(string domain, string name)
		{
			//_securityManager.CurrentUser.IsAuthorized()
			return await ApplicationDataProvider.ExecuteAsync(
							async (session, context) =>
							{
								return await session.GetGroupsAsync(context.domain, context.name);
							},
							context: new { domain, name },
							TransactionScopeOption.Required,
							new TransactionOptions()
							{
								IsolationLevel = IsolationLevel.ReadCommitted
							}
						);
		}

		private static async Task<IGroup> _GetGroupAsync(IDataSession dataSession, string @group, IApplicationData<IDataSession> applicationData, ISecurityManager securityManager)
		{
			if(await dataSession.GetGroupInfoAsync(@group) == null)
				throw new UnknownGroupException(@group);

			IGroup grp = new Group(@group, applicationData, securityManager);
			
			return grp;
		}
		
		private static IGroup _GetGroup(IDataSession dataSession, string @group, IApplicationData<IDataSession> applicationData, ISecurityManager securityManager)
		{
			if(dataSession.GetGroupInfo(@group) == null)
				throw new UnknownGroupException(@group);

			IGroup grp = new Group(@group, applicationData, securityManager);
			
			return grp;
		}

		private async Task<IGroup> _GetGroupAsync(string identifier)
		{
			return await  ApplicationDataProvider.ExecuteAsync(
					function: async (dataSession, context) =>
					{
						return await _GetGroupAsync(dataSession, context.identifier, context.@this.ApplicationDataProvider,
										context.@this.SecurityManager);
					},
					new { identifier, @this = this },
					TransactionScopeOption.Required,
					new TransactionOptions()
					{
						IsolationLevel = IsolationLevel.ReadCommitted
					}
				).ConfigureAwait(false);
		}

		private IGroup _GetGroup(string identifier)
		{
			return ApplicationDataProvider.Execute(
					function: (dataSession, context) =>
					{
						return _GetGroup(dataSession, context.identifier, context.@this.ApplicationDataProvider,
										context.@this.SecurityManager);
					},
					new { identifier, @this = this },
					TransactionScopeOption.Required,
					new TransactionOptions()
					{
						IsolationLevel = IsolationLevel.ReadCommitted
					}
				);
		}

		private async Task _DeleteGroupAsync(string identifier)
		{
			await ApplicationDataProvider.PerformAsync(
					async (session, context) =>
					{
						await session.DeleteAsync(context.identifier).ConfigureAwait(false);
					},
					context: new { identifier },
					TransactionScopeOption.Required,
					new TransactionOptions()
					{
						IsolationLevel = IsolationLevel.ReadCommitted
					}
				);
		}

		private async Task<IEnumerable<DirectMembership>> _GetMemberships(string domain, Subject subject,
																		bool recursive = false)
		{
			return await ApplicationDataProvider.ExecuteAsync(
							async (session, context) =>
							{
								if(!context.recursive)
									return await session.GetMembershipsAsync(context.subject, context.domain);

								return await Helpers.GetMembershipsAsync(context.subject, context.domain, session);
							},
							context: new { domain, subject, recursive },
							TransactionScopeOption.Required,
							new TransactionOptions()
							{
								IsolationLevel = IsolationLevel.ReadCommitted
							}

						);
		}

		private async Task _AddToGroupsAsync(Subject subject, IEnumerable<string> groups)
		{
			await ApplicationDataProvider.PerformAsync(
					action: async (dataSession, context) =>
					{
						foreach(string group in context.groups)
						{
							Group grp = await _GetGroupAsync(dataSession, @group, context.@this.ApplicationDataProvider, context.@this.SecurityManager) as Group;

							await grp._AddMembersAsync(new[] { context.subject });
						}
					},
					new {@this = this, subject, groups},
					TransactionScopeOption.Required,
					new TransactionOptions()
					{
						IsolationLevel = IsolationLevel.ReadCommitted
					}
				);
		}
	}
}