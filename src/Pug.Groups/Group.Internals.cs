
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Pug.Application.Data;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups
{
	public partial class Group
	{
		private readonly string _domain;
		
		private async Task<GroupInfo> _GetDefinitionAsync()
		{
			GroupInfo info = await ApplicationDataProvider.ExecuteAsync(
									async (dataSession, context) =>
									{
										return await dataSession.GetGroupDefinitionAsync(context.@this.Identifier)
																.ConfigureAwait(false);
									},
									context: new { @this = this },
									TransactionScopeOption.Required,
									new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
								).ConfigureAwait(false);
			return info;
		}
		
		private async Task<GroupInfo> _GetInfoAsync()
		{
			GroupInfo info = await ApplicationDataProvider.ExecuteAsync(
									async (dataSession, context) =>
									{
										return await dataSession.GetGroupInfoAsync(context.@this.Identifier)
																.ConfigureAwait(false);
									},
									context: new { @this = this },
									TransactionScopeOption.Required,
									new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
								).ConfigureAwait(false);
			return info;
		}
		
		private GroupInfo _GetInfo()
		{
			return ApplicationDataProvider.Execute(
									(dataSession, context) =>
									{
										return dataSession.GetGroupInfo(context.@this.Identifier);
									},
									context: new { @this = this },
									TransactionScopeOption.Required,
									new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
								);
		}
		
		internal async Task _AddMembersAsync(IEnumerable<Subject> subjects)
		{
			await ApplicationDataProvider.PerformAsync(
					action: async (dataSession, context) =>
					{
						foreach(Subject subject in context.subjects)
						{
							if(subject == null)
								continue;

							Membership membership = new Membership()
							{
								Assignor = context.@this.SecurityManager.CurrentUser.Identity.Identifier,
								Subject = subject,
								Group = context.@this.Identifier,
								AssignmentTimestamp = DateTime.Now
							};

							await dataSession.InsertAsync(membership);
						}
					},
					new { subjects, @this = this },
					TransactionScopeOption.Required,
					new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
				).ConfigureAwait(false);

		}

		private async Task<bool> _HasMemberAsync(Subject subject, bool recursive)
		{
			return await ApplicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await Helpers.GroupHasMemberAsync(context.@this.Identifier, context.subject,
																		context.recursive, dataSession).ConfigureAwait(false);
							},
							context: new {@this = this, subject, recursive},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
						).ConfigureAwait(false);
		}

		private async Task _RemoveMemberAsync(Subject subject)
		{
			await ApplicationDataProvider.PerformAsync(
					action: async (dataSession, context) => { await dataSession.DeleteAsync(context.@this.Identifier, context.subject); },
					new { subject, @this = this },
					TransactionScopeOption.Required,
					new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
				).ConfigureAwait(false);
		}

		private async Task<IEnumerable<Membership>> _GetMembershipsAsync()
		{
			return await ApplicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await dataSession.GetMembershipsAsync(context.@this.Identifier)
														.ConfigureAwait(false);
							},
							context: new {@this = this},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
						).ConfigureAwait(false);
		}
	}
}