using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pug.Application.Data;
using Pug.Effable;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups.PostgresData
{
	public class DapperDataSession : ApplicationDataSession, IDataSession
	{
		public DapperDataSession( IDbConnection databaseSession ) : base( databaseSession )
		{
		}

		private const string GroupInsertQuery = 
			@"insert into group(identifier, domain, name, description, registrationUser, registrationTimestamp)
				values(@identifier, @domain, @name, @description, @registrationUser, @registrationTimestamp)";

		private static object GetGroupInsertParameters( GroupInfo groupInfo )
		{
			return new
			{
				groupInfo.Identifier, groupInfo.Definition.Domain, groupInfo.Definition.Name, groupInfo.Definition.Description,
				groupInfo.RegistrationInfo.User, groupInfo.RegistrationInfo.Timestamp
			};
		}
		
		public void Insert( GroupInfo groupInfo )
		{
			Connection.Execute(
					GroupInsertQuery,
					GetGroupInsertParameters( groupInfo )
				);
		}

		public Task InsertAsync( GroupInfo groupInfo )
		{
			return Connection.ExecuteAsync(
					GroupInsertQuery,
					GetGroupInsertParameters( groupInfo )
				);
		}

		private const string GetGroupsQuery = 
			@"select identifier, domain, name, description, registrationUser as user, registrationTimestamp as timestamp, lastUpdateUser as user, lastUpdateTimestamp as timestamp
				from group
				where domain = @domain and (name = @name or @name = '' or @name is null);";

		private readonly Func<string, GroupDefinition, ActionContext<string>, ActionContext<string>, GroupInfo> MapToGroupInfo = ( identifier, definition, registrationInfo, lastUpdateInfo ) =>
			new GroupInfo()
			{
				Identifier = identifier,
				Definition = definition,
				RegistrationInfo = registrationInfo,
				LastUpdateInfo = lastUpdateInfo
			};
		
		public Task<IEnumerable<GroupInfo>> GetGroupsAsync( string domain, string name )
		{
			return Connection.QueryAsync(
					GetGroupsQuery,
					param: new { domain, name },
					splitOn: "domain, user, user",
					map: MapToGroupInfo 
				);
		}

		public Task<GroupDefinition> GetGroupDefinitionAsync( string identifier )
		{
			return
				Connection.QuerySingleOrDefaultAsync<GroupDefinition>(
						@"select identifier, domain, name, description
							from group
							where identifier = @identifier;",
						param: new { identifier }
					);
		}

		private const string GetGroupInfoQuery = 
			@"select identifier, domain, name, description, registrationUser, registrationTimestamp, lastUpdateUser, lastUpdateTimestamp
				from group
				where identifier = @identifier;";

		public async Task<GroupInfo> GetGroupInfoAsync( string identifier )
		{
			return ( await
						Connection.QueryAsync(
								GetGroupInfoQuery,
								param: new { identifier },
								splitOn: "domain, registrationUser, lastUpdateUser",
								map: MapToGroupInfo
							)
					).FirstOrDefault();
		}

		public GroupInfo GetGroupInfo( string identifier )
		{
			return Connection.Query(
									GetGroupInfoQuery,
									param: new { identifier },
									splitOn: "domain, user, user",
									map: MapToGroupInfo
								)
							.FirstOrDefault();
		}

		private const string GetGroupMembershipsQuery =
			@"select subjectType, subjectIdentifier, group, registrationTimestamp as timestamp, registrationUser as user
				from membership where group = @group";

		private readonly Func<MembershipDefinition, ActionContext<string>, Membership> ConstructMembership =
			( definition, registrationInfo ) =>
				new Membership()
				{
					Group = definition.Group,
					Subject = definition.Subject,
					RegistrationInfo = registrationInfo
				};
		
		public Task<IEnumerable<Membership>> GetMembershipsAsync( string group )
		{
			return Connection.QueryAsync(
				GetGroupMembershipsQuery,
				param: new { group },
				splitOn: "timestamp",
				map: ConstructMembership );
		}

		public IEnumerable<Membership> GetMemberships( string group )
		{
			return Connection.Query(
				GetGroupMembershipsQuery,
				param: new { group },
				splitOn: "timestamp",
				map: ConstructMembership );
		}
		
		private const string GetSubjectMembershipsQuery =
			@"select subjectType, subjectIdentifier, group, registrationTimestamp as timestamp, registrationUser as userr
				from membership 
				where subjectType = @subjectType and subjectIdentifier = @subjectIdentifier and (domain = @domain or coalesce(@domain, '') = '')";

		public Task<IEnumerable<Membership>> GetMembershipsAsync( Subject subject, string domain = null )
		{
			return Connection.QueryAsync(
				GetSubjectMembershipsQuery,
				param: new { subjectType = subject.Type, subjectIdentifier = subject.Identifier, domain },
				splitOn: "timestamp",
				map: ConstructMembership );
		}

		public IEnumerable<Membership> GetMemberships( Subject subject, string domain = null )
		{
			return Connection.Query(
				GetSubjectMembershipsQuery,
				param: new { subjectType = subject.Type, subjectIdentifier = subject.Identifier, domain },
				splitOn: "timestamp",
				map: ConstructMembership );
		}
	
		private const string GetSubjectGroupMembershipQuery =
			@"select subjectType, subjectIdentifier, group, registrationTimestamp as timestamp, registrationUser as user
				from membership 
				where subjectType = @subjectType and subjectIdentifier = @subjectIdentifier and group = @group";

		public Membership GetMembership( Subject subject, string group )
		{
			return Connection.Query(
								GetSubjectGroupMembershipQuery,
								param: new { subjectType = subject.Type, subjectIdentifier = subject.Identifier, group },
								splitOn: "timestamp",
								map: ConstructMembership )
							.FirstOrDefault();
		}

		public async Task<Membership> GetMembershipAsync( Subject subject, string group )
		{
			return ( await
						Connection.QueryAsync(
							GetSubjectGroupMembershipQuery,
							param: new { subjectType = subject.Type, subjectIdentifier = subject.Identifier, group },
							splitOn: "registrationTimestamp",
							map: ConstructMembership )
					).FirstOrDefault();
		}

		public Task InsertAsync( Membership membership )
		{
			return Connection.ExecuteAsync(
					@"insert into membership(subjectType, subjectIdentifier, group, registrationUser as user, registrationTimestamp as timestamp)
							values(@subjectType, @subjectIdentifier, @group, @registrationUser, @registrationTimestamp)",
					param: new
					{
						subjectType = membership.Subject.Type,
						subjectIdentifier = membership.Subject.Identifier,
						membership.Group,
						membership.RegistrationInfo.User,
						membership.RegistrationInfo.Timestamp
					}
				);
		}
		
		public Task DeleteAsync( string @group, Subject subject )
		{
			return Connection.ExecuteAsync(
					@"delete from membership 
							where subjectType = @subjectType and subjectIdentifier = @subjectIdentifier and group = @group",
					param: new { subjectType = subject.Type, subjectIdentifier = subject.Identifier, group }
				);
		}

		public Task DeleteAsync( string @group )
		{
			return Connection.ExecuteAsync(
					"delete from group where identifier = @group",
					param: new { group }
				);
		}
	}
}