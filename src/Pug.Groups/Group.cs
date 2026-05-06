using Pug.Application.Data;
using Pug.Application.Data.Extensions;
using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups
{
	public partial class Group : GroupsBase, IGroup
	{
		public string Identifier { get; }

		public Group(
			string identifier, string domain, IApplicationData<IDataSession> applicationDataProvider,
			ISecurityManager securityManager
		)
			: base( applicationDataProvider, securityManager )
		{
			if( string.IsNullOrWhiteSpace( identifier ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof(identifier) );

			Identifier = identifier;
			_domain = domain;
		}

		public async Task<GroupDefinition> GetDefinitionAsync()
		{
			await CheckAuthorizationAsync(
					SecurityOperations.GetDefinition,
					new NounQualifier()
					{
						Domain = _domain,
							Type = SecurityObjectTypes.Group,
							Identifier = Identifier
					}
				)
				.ConfigureAwait( false );

			return await _GetDefinitionAsync().ConfigureAwait( false );
		}

		public async Task<GroupInfo> GetInfoAsync()
		{
			await CheckAuthorizationAsync(
					SecurityOperations.GetInfo,
					new NounQualifier()
					{
						Domain = _domain,
							Type = SecurityObjectTypes.Group,
							Identifier = Identifier
					}
				)
				.ConfigureAwait( false );

			return await ApplicationDataProvider.ExecuteAsync(
													( dataSession, context ) =>
													{
														return dataSession.GetGroupInfoAsync(
															context.@this.Identifier
														);
													},
													context: new { @this = this }
												)
												.ConfigureAwait( false );
		}

		public async Task<IEnumerable<Membership>> GetMembershipsAsync()
		{
			await CheckAuthorizationAsync(
					SecurityOperations.ListMemberships,
					new NounQualifier()
					{
						Domain = _domain,
						Type = SecurityObjectTypes.Group,
						Identifier = Identifier
					}
				)
				.ConfigureAwait( false );

			return await _GetMembershipsAsync().ConfigureAwait( false );
		}

		public async Task<bool> HasMemberAsync( Subject subject, bool recursive = false )
		{
			Helpers.ValidateParameter( subject, nameof(subject) );

			await CheckAuthorizationAsync(
					SecurityOperations.ListMemberships,
					new NounQualifier()
					{
						Domain = _domain,
						Type = SecurityObjectTypes.Group,
						Identifier = Identifier
					}
				)
				.ConfigureAwait( false );

			return await _HasMemberAsync( subject, recursive ).ConfigureAwait( false );
		}

		public async Task AddMembersAsync( IEnumerable<Subject> subjects )
		{
			if( subjects == null || !subjects.Any() )
				return;

			foreach( Subject subject in subjects )
			{
				if( subject == null )
					continue;

				Helpers.ValidateParameter( subject, nameof(subjects) );
			}

			await CheckAuthorizationAsync(
					SecurityOperations.CreateMembership,
					new NounQualifier()
					{
						Domain = _domain,
							Type = SecurityObjectTypes.Group,
							Identifier = Identifier
					}
				)
				.ConfigureAwait( false );

			await _AddMembersAsync( subjects ).ConfigureAwait( false );
		}

		public Task RemoveMemberAsync( Subject subject )
		{
			if( subject == null ) throw new ArgumentNullException( nameof(subject) );

			Helpers.ValidateParameter( subject, nameof(subject) );

			return _RemoveMemberAsync( subject );
		}
	}
}