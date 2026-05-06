using Pug.Application.Data;
using Pug.Application.Data.Extensions;
using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups
{
	public partial class Groups : IGroups
	{
		public Groups( IdentifierGenerator identifierGenerator,
						IApplicationData<IDataSession> applicationDataProvider,
						ISecurityManager securityManager
		)
			: base( applicationDataProvider, securityManager )
		{
			_identifierGenerator = identifierGenerator ?? throw new ArgumentNullException( nameof(identifierGenerator) );

		}

		private async Task<string> _AddGroupAsync( GroupInfo groupInfo )
		{
			await CheckAuthorizationAsync(
					SecurityOperations.Create,
					ResourceQualifiers.AnyGroup( groupInfo.Definition.Domain)
				)
				.ConfigureAwait( false );

			return await ApplicationDataProvider.ExecuteAsync(
						async ( session, context ) =>
						{
							await session.InsertAsync( context.groupInfo ).ConfigureAwait( false );

							return context.groupInfo.Identifier;
						},
						context: new { groupInfo }
					).ConfigureAwait( false );
		}

		public Task<string> AddGroupAsync( GroupDefinition definition )
		{
			if( definition == null ) throw new ArgumentNullException( nameof(definition) );

			string identifier = _identifierGenerator.GetNext();

			GroupInfo groupInfo = new()
			{ Identifier = identifier, Definition = definition };

			return _AddGroupAsync( groupInfo );
		}

		private async Task<IEnumerable<GroupInfo>> _GetGroupsAsync( string domain, string name )
		{
			await CheckAuthorizationAsync(
					SecurityOperations.List,
					ResourceQualifiers.AnyGroup( domain)
				)
				.ConfigureAwait( false );
			
			return await ApplicationDataProvider.ExecuteAsync(
						async ( session, context ) => { return await session.GetGroupsAsync( context.domain, context.name ).ConfigureAwait( false ); },
						context: new { domain, name }
					).ConfigureAwait( false );
		}

		public Task<IEnumerable<GroupInfo>> GetGroupsAsync( string domain, string name = null )
		{
			if( domain == null ) throw new ArgumentNullException( nameof(domain) );

			return _GetGroupsAsync( domain, name );
		}

		private async Task<IGroup> _GetGroupAsync( string identifier )
		{
			return await ApplicationDataProvider.ExecuteAsync(
							function: async ( dataSession, context ) =>
							{
								return await _GetGroupAsync( dataSession, context.identifier, context.@this.ApplicationDataProvider,
															context.@this.SecurityManager ).ConfigureAwait( false );
							},
							new { identifier, @this = this }
						).ConfigureAwait( false );
		}

		public Task<IGroup> GetGroupAsync( string identifier )
		{
			if( string.IsNullOrWhiteSpace( identifier ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof(identifier) );

			return _GetGroupAsync( identifier );
		}

		private async Task _DeleteGroupAsync( string identifier )
		{
			await ApplicationDataProvider.PerformAsync(
				async ( session, context ) =>
				{
					GroupInfo info = await session.GetGroupInfoAsync(context.identifier).ConfigureAwait( false );

					if( info == null )
						return;

					await context.@this.CheckAuthorizationAsync(
							SecurityOperations.Delete,
							new NounQualifier()
							{
								Domain = info.Definition.Domain,
									Type = SecurityObjectTypes.Group,
									Identifier = context.identifier
							}
						)
						.ConfigureAwait( false );
					
					await session.DeleteAsync( context.identifier ).ConfigureAwait( false );
				},
				context: new {@this = this, identifier }
			).ConfigureAwait( false );
		}

		public Task DeleteGroupAsync( string identifier )
		{
			if( string.IsNullOrWhiteSpace( identifier ) ) throw new ArgumentException( "Value cannot be null or whitespace.", nameof(identifier) );

			return _DeleteGroupAsync( identifier );
		}

		private async Task<IEnumerable<Membership>> _GetMemberships( Subject subject, string domain,
																			bool recursive = false )
		{
			await CheckAuthorizationAsync(
					SecurityOperations.ListMemberships,
					ResourceQualifiers.AnyGroup( domain)
				)
				.ConfigureAwait( false );

			return await ApplicationDataProvider.ExecuteAsync(
						async ( session, context ) =>
						{
							if( !context.recursive )
								return await session.GetMembershipsAsync( context.subject, context.domain ).ConfigureAwait( false );

							return await Helpers.GetMembershipsAsync( context.subject, context.domain, session ).ConfigureAwait( false );
						},
						context: new { subject, recursive, domain }

					).ConfigureAwait( false );
		}

		public Task<IEnumerable<Membership>> GetMemberships( string domain, Subject subject,
																	bool recursive = false )
		{
			if( subject == null ) throw new ArgumentNullException( nameof(subject) );

			if( string.IsNullOrWhiteSpace( subject.Type ) || string.IsNullOrWhiteSpace( subject.Identifier ) )
				throw new ArgumentException( "Subject type and identifier must be specified", nameof(subject) );

			return _GetMemberships( subject, domain, recursive );
		}

		private async Task _AddToGroupsAsync( Subject subject, IEnumerable<string> groups )
		{
			await ApplicationDataProvider.PerformAsync(
				action: async ( dataSession, context ) =>
				{
					foreach( string group in context.groups )
					{
						GroupInfo info = await dataSession.GetGroupInfoAsync( group ).ConfigureAwait( false );

						if( info == null )
							throw new UnknownGroupException( group );

						await CheckAuthorizationAsync(
								SecurityOperations.CreateMembership,
								new NounQualifier()
								{
									Domain = info.Definition.Domain,
									Type = SecurityObjectTypes.Group,
									Identifier = group
								}
							)
							.ConfigureAwait( false );
					}

					foreach( string group in context.groups )
					{
						await Helpers.RegisterMembershipAsync(
							subject,
							group,
							context.@this.SecurityManager.CurrentUser,
							dataSession
						).ConfigureAwait( false );
					}
				},
				new { @this = this, subject, groups }
			).ConfigureAwait( false );
		}

		public Task AddToGroupsAsync( Subject subject, IEnumerable<string> groups )
		{
			if( subject == null ) throw new ArgumentNullException( nameof(subject) );

			Helpers.ValidateParameter( subject, nameof(subject) );

			if( groups == null || !groups.Any() )
				return Task.CompletedTask;

			return _AddToGroupsAsync( subject, groups );
		}
	}
}