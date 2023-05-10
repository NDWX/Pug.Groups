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
			await CheckAuthorizationAsync( groupInfo.Definition.Domain, SecurityOperations.Create, SecurityObjectTypes.Group );

			return await ApplicationDataProvider.ExecuteAsync(
							async ( session, context ) =>
							{
								await session.InsertAsync( context.groupInfo ).ConfigureAwait( false );

								return context.groupInfo.Identifier;
							},
							context: new { groupInfo }
						);
		}

		public Task<string> AddGroupAsync( GroupDefinition definition )
		{
			if( definition == null ) throw new ArgumentNullException( nameof(definition) );

			string identifier = _identifierGenerator.GetNext();

			GroupInfo groupInfo = new ()
				{ Identifier = identifier, Definition = definition };

			return _AddGroupAsync( groupInfo );
		}

		private async Task<IEnumerable<GroupInfo>> _GetGroupsAsync( string domain, string name )
		{
			await CheckAuthorizationAsync( domain, SecurityOperations.List, SecurityObjectTypes.Group );
			
			return await ApplicationDataProvider.ExecuteAsync(
							async ( session, context ) => { return await session.GetGroupsAsync( context.domain, context.name ); },
							context: new { domain, name }
						);
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
															context.@this.SecurityManager );
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
			IGroup grp = new Group( identifier, ApplicationDataProvider, SecurityManager );
			GroupInfo info = await grp.GetInfoAsync();

			if( info == null )
				return;

			await CheckAuthorizationAsync( info.Definition.Domain, SecurityOperations.Delete, SecurityObjectTypes.Group, identifier );

			await ApplicationDataProvider.PerformAsync(
					async ( session, context ) => { await session.DeleteAsync( context.identifier ).ConfigureAwait( false ); },
					context: new { identifier }
				);
		}

		public Task DeleteGroupAsync( string identifier )
		{
			if( string.IsNullOrWhiteSpace( identifier ) ) throw new ArgumentException( "Value cannot be null or whitespace.", nameof(identifier) );

			return _DeleteGroupAsync( identifier );
		}

		private async Task<IEnumerable<Membership>> _GetMemberships( string domain, Subject subject,
																			bool recursive = false )
		{

			await CheckAuthorizationAsync( domain, SecurityOperations.ListMemberships, SecurityObjectTypes.Subject );

			return await ApplicationDataProvider.ExecuteAsync(
							async ( session, context ) =>
							{
								if( !context.recursive )
									return await session.GetMembershipsAsync( context.subject, context.domain );

								return await Helpers.GetMembershipsAsync( context.subject, context.domain, session );
							},
							context: new { domain, subject, recursive }

						);
		}

		public Task<IEnumerable<Membership>> GetMemberships( string domain, Subject subject,
																	bool recursive = false )
		{
			if( domain == null ) throw new ArgumentNullException( nameof(domain) );
			if( subject == null ) throw new ArgumentNullException( nameof(subject) );

			if( string.IsNullOrWhiteSpace( subject.Type ) || string.IsNullOrWhiteSpace( subject.Identifier ) )
				throw new ArgumentException( "Subject type and identifier must be specified", nameof(subject) );

			return _GetMemberships( domain, subject, recursive );
		}

		private async Task _AddToGroupsAsync( Subject subject, IEnumerable<string> groups )
		{
			foreach( string group in groups )
			{
				IGroup grp = new Group( group, ApplicationDataProvider, SecurityManager );
				GroupInfo info = await grp.GetInfoAsync();

				if( info == null )
					throw new UnknownGroupException( group );

				await CheckAuthorizationAsync( info.Definition.Domain, SecurityOperations.CreateMembership, SecurityObjectTypes.Group, group );
			}

			await ApplicationDataProvider.PerformAsync(
					action: async ( dataSession, context ) =>
					{
						foreach( string group in context.groups )
						{
							if( await dataSession.GetGroupInfoAsync( group ) == null )
								throw new UnknownGroupException( group );
						}

						foreach( string group in context.groups )
						{
							Group grp = await _GetGroupAsync( dataSession, @group, context.@this.ApplicationDataProvider,
															context.@this.SecurityManager ) as Group;

							// ReSharper disable once PossibleNullReferenceException
							await grp._AddMembersAsync( new[] { context.subject } );
						}
					},
					new { @this = this, subject, groups }
				);
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