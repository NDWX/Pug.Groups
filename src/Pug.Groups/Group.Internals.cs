
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pug.Application.Data;
using Pug.Effable;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups
{
	public partial class Group
	{
		private readonly string _domain;
		
		private async Task<GroupDefinition> _GetDefinitionAsync()
		{
			return await ApplicationDataProvider.ExecuteAsync(
									(dataSession, context) =>
									{
										return dataSession.GetGroupDefinitionAsync( context.@this.Identifier );
									},
									context: new { @this = this }
								).ConfigureAwait(false);
		}

		private GroupInfo _GetInfo()
		{
			return ApplicationDataProvider.Execute(
									(dataSession, context) =>
									{
										return dataSession.GetGroupInfo(context.@this.Identifier);
									},
									context: new { @this = this }
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

							Membership membership = new ()
							{
								Subject = subject,
								Group = context.@this.Identifier,
								RegistrationInfo = new ActionContext<string>()
								{
									Timestamp = DateTime.Now,
									User = context.@this.SecurityManager.CurrentUser.Identity.Identifier
								}
							};

							await dataSession.InsertAsync(membership);
						}
					},
					new { subjects, @this = this }
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
							context: new {@this = this, subject, recursive}
						).ConfigureAwait(false);
		}

		private async Task _RemoveMemberAsync( Subject subject )
		{
			await CheckAuthorizationAsync( _domain, SecurityOperations.DeleteMembership, SecurityObjectTypes.Group, Identifier );

			await ApplicationDataProvider.PerformAsync(
					action: async (dataSession, context) => { await dataSession.DeleteAsync(context.@this.Identifier, context.subject); },
					new { subject, @this = this }
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
							context: new {@this = this}
						).ConfigureAwait(false);
		}
	}
}