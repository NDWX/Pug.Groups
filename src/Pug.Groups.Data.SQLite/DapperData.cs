using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Pug.Application.Data;
using Pug.Groups.Common;
using static System.String;

namespace Pug.Groups.SQLiteData
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
									@"create table ""group""(
											    identifier text not null,
											    domain text not null default '',
											    name text not null,
											    description text not null default '',
											    registrationTimestamp text not null default current_timestamp,
											    registrationUser text not null,
											    lastUpdateTimestamp text,
											    lastUpdateUser text not null default '',
											    primary key (identifier),
											    unique (domain, name)
											);

											create index group_registrationUser_idx on ""group""(registrationUser);
											create index group_lastUpdateUser_idx on ""group""(lastUpdateUser);",
									@"drop table ""group"";"
								),
							new UpgradeScript(
								"Membership table", Empty,
								@"create table membership(
										    subjectType text not null,
										    subjectIdentifier text not null,
										    ""group"" text not null,
										    registrationTimestamp text not null,
										    registrationUser text not null,
										    primary key (subjectType, subjectIdentifier, ""group"")
										);

										create index membership_registrationUser_idx on membership(registrationUser);
										create index membership_group_idx on membership(""group"");
										create index membership_subject_idx on membership(subjectType, subjectIdentifier);",
								@"drop table membership;" )
						}
					)
			};
		}
	}
}
