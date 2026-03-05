using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Pug.Application.Data;
using Pug.Groups.Common;
using static System.String;

namespace Pug.Groups.PostgresData
{
	public class DapperData : ApplicationData<IDataSession>
	{
		public DapperData( string location, DbProviderFactory dataProvider ) : base( location, dataProvider )
		{
		}

		protected override IDataSession CreateApplicationDataSession( IDbConnection databaseSession, DbProviderFactory dataAccessProvider )
		{
			return new DapperDataSession( databaseSession );
		}

		protected override IEnumerable<SchemaVersion> InitializeUpgradeScripts()
		{
			return new[]
			{
				new SchemaVersion(
						1, new[]
						{
							new UpgradeScript(
									"Group table", Empty,
									@"create table group(
											    identifier character varying not null,
											    domain character varying not null default '',
											    name character varying not null,
											    description character varying not null default '',
											    registrationTimestamp timestamp with time zone not null default current_timestamp,
											    registrationUser character varying not null,
											    lastUpdateTimestamp timestamp with time zone,
											    lastUpdateUser character varying not null default '',
											    primary key (identifier),
											    constraint unique (domain, name)						    
											);

											create index group_registrationUser_idx on group(registrationUser);
											create index group_lastUpdateUser_idx on group(lastUpdateUser);",
									@"drop table group;"
								),
							new UpgradeScript(
								"Membership table", Empty,
								@"create table membership(
										    subjectType character varying not null, 
										    subjectIdentifier character varying not null,
										    group character varying not null,
										    registrationTimestamp timestamp with time zone not null,
										    registrationUser character varying not null,
										    primary key (subjectType, subjectIdentifier, group)						    
										);

										create index membership_registrationUser_idx ON membership(registrationUser);
										create index membership_group_idx on membership(group);
										create index membership_subject_idx on membership(subjectType, subjectIdentifier);",
								"drop table membership;" )
						}
					)
			};
		}
	}
}